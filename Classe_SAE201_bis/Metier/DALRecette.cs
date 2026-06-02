using Classe_SAE201_bis.Metier;
using Npgsql;
using System.Collections.Generic;

namespace Classe_SAE201_bis.DAL
{
    public class DALRecette
    {
        public static List<Recette> GetToutes()
        {
            var recettes = new List<Recette>();
            string sql = @"SELECT r.recette_id, r.recette_nom, r.recette_description,
                                  c.categorie_id, c.categorie_nom
                           FROM recette r
                           JOIN categorie c ON r.categorie_id = c.categorie_id
                           ORDER BY c.categorie_nom, r.recette_nom";
            using var cmd = new NpgsqlCommand(sql, DALConnexion.GetConnexion());
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var cat = new Categorie(reader.GetInt32(3), reader.GetString(4));
                recettes.Add(new Recette(
                    reader.GetInt32(0), reader.GetString(1),
                    reader.IsDBNull(2) ? "" : reader.GetString(2), cat));
            }
            return recettes;
        }

        public static List<Recette> GetParCategorie(int categorieId)
        {
            var recettes = new List<Recette>();
            string sql = @"SELECT r.recette_id, r.recette_nom, r.recette_description,
                                  c.categorie_id, c.categorie_nom
                           FROM recette r
                           JOIN categorie c ON r.categorie_id = c.categorie_id
                           WHERE r.categorie_id = @id
                           ORDER BY r.recette_nom";
            using var cmd = new NpgsqlCommand(sql, DALConnexion.GetConnexion());
            cmd.Parameters.AddWithValue("@id", categorieId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var cat = new Categorie(reader.GetInt32(3), reader.GetString(4));
                recettes.Add(new Recette(
                    reader.GetInt32(0), reader.GetString(1),
                    reader.IsDBNull(2) ? "" : reader.GetString(2), cat));
            }
            return recettes;
        }

        public static int Ajouter(Recette recette)
        {
            string sql = @"INSERT INTO recette (categorie_id, recette_nom, recette_description)
                           VALUES (@cat, @nom, @desc) RETURNING recette_id";
            using var cmd = new NpgsqlCommand(sql, DALConnexion.GetConnexion());
            cmd.Parameters.AddWithValue("@cat", recette.Categorie.CategorieId);
            cmd.Parameters.AddWithValue("@nom", recette.RecetteNom);
            cmd.Parameters.AddWithValue("@desc", recette.RecetteDescription ?? "");
            return (int)cmd.ExecuteScalar();
        }
    }
}
