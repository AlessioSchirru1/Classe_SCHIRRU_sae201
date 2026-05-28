using Classe_SAE201_bis.Metier;
using Npgsql;
using System.Collections.Generic;

namespace Classe_SAE201_bis.DAL
{
    public class DALProduit
    {
        /// <summary>
        /// Retourne tous les produits disponibles, avec recette, catégorie et allergènes.
        /// </summary>
        public static List<Produit> GetTous( bool inclureIndisponibles = false )
        {
            var produits = new List<Produit>();
            string sql = @"
                SELECT p.produit_id, p.est_indisponible, p.nb_parts, p.prix,
                       r.recette_id, r.recette_nom, r.recette_description,
                       c.categorie_id, c.categorie_nom
                FROM produit p
                JOIN recette r ON p.recette_id = r.recette_id
                JOIN categorie c ON r.categorie_id = c.categorie_id
                " + (inclureIndisponibles ? "" : "WHERE p.est_indisponible = false") + @"
                ORDER BY c.categorie_nom, r.recette_nom, p.nb_parts";

            using var cmd = new NpgsqlCommand(sql, DALConnexion.GetConnexion());
            using var reader = cmd.ExecuteReader();
            while(reader.Read())
            {
                var cat = new Categorie(reader.GetInt32(7), reader.GetString(8));
                var rec = new Recette(reader.GetInt32(4), reader.GetString(5),
                                     reader.IsDBNull(6) ? "" : reader.GetString(6), cat);
                produits.Add(new Produit(
                    reader.GetInt32(0), rec,
                    reader.GetInt32(2), (double)reader.GetDecimal(3),
                    reader.GetBoolean(1)
                ));
            }
            return produits;
        }

        /// <summary>
        /// Ajoute un nouveau produit au catalogue.
        /// </summary>
        public static void Ajouter( Produit produit )
        {
            string sql = @"INSERT INTO produit (recette_id, est_indisponible, nb_parts, prix)
                           VALUES (@recette, @indispo, @parts, @prix)";
            using var cmd = new NpgsqlCommand(sql, DALConnexion.GetConnexion());
            cmd.Parameters.AddWithValue("@recette", produit.Recette.RecetteId);
            cmd.Parameters.AddWithValue("@indispo", produit.EstIndisponible);
            cmd.Parameters.AddWithValue("@parts", produit.NbParts);
            cmd.Parameters.AddWithValue("@prix", produit.Prix);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Modifie le prix et la disponibilité d'un produit.
        /// </summary>
        public static void Modifier( Produit produit )
        {
            string sql = @"UPDATE produit SET prix = @prix, est_indisponible = @indispo
                           WHERE produit_id = @id";
            using var cmd = new NpgsqlCommand(sql, DALConnexion.GetConnexion());
            cmd.Parameters.AddWithValue("@prix", produit.Prix);
            cmd.Parameters.AddWithValue("@indispo", produit.EstIndisponible);
            cmd.Parameters.AddWithValue("@id", produit.ProduitId);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Rend un produit indisponible (pas de suppression physique).
        /// </summary>
        public static void RendreIndisponible( int produitId )
        {
            string sql = "UPDATE produit SET est_indisponible = true WHERE produit_id = @id";
            using var cmd = new NpgsqlCommand(sql, DALConnexion.GetConnexion());
            cmd.Parameters.AddWithValue("@id", produitId);
            cmd.ExecuteNonQuery();
        }
    }
}
