using Classe_SAE201_bis.Metier;
using Npgsql;
using System;
using System.Collections.Generic;

namespace Classe_SAE201_bis.DAL
{
    public class DALCommande
    {
        public static List<Commande> GetCommandesDuJour()
        {
            return GetCommandes("WHERE c.date_retrait = CURRENT_DATE");
        }

        public static List<Commande> GetToutes()
        {
            return GetCommandes("");
        }

        private static List<Commande> GetCommandes( string filtre )
        {
            var commandes = new List<Commande>();
            string sql = $@"
                SELECT c.commande_id, c.date_creation, c.date_retrait,
                       c.acompte, c.est_prete, c.est_recuperee,
                       c.total, c.nb_personne, c.date_evenement,
                       cl.client_id, cl.nom, cl.prenom, cl.telephone, cl.mail,
                       ce.categorie_evenement_id, ce.categorie_evenement_nom
                FROM commande c
                JOIN client cl ON c.client_id = cl.client_id
                LEFT JOIN categorie_evenement ce 
                       ON c.categorie_evenement_id = ce.categorie_evenement_id
                {filtre}
                ORDER BY c.date_retrait, c.commande_id";

            using var cmd = new NpgsqlCommand(sql, DALConnexion.GetConnexion());
            using var reader = cmd.ExecuteReader();
            while(reader.Read())
            {
                var client = new Client(
                    reader.GetInt32(9), reader.GetString(10),
                    reader.IsDBNull(11) ? "" : reader.GetString(11),
                    reader.GetString(12),
                    reader.IsDBNull(13) ? "" : reader.GetString(13)
                );
                CategorieEvenement catEvt = null;
                if(!reader.IsDBNull(14))
                    catEvt = new CategorieEvenement(reader.GetInt32(14), reader.GetString(15));

                var commande = new Commande(
                    reader.GetInt32(0),
                    reader.GetDateTime(2),
                    reader.IsDBNull(7) ? 0 : reader.GetInt32(7),
                    client, catEvt
                );
                commande.DateCreation = reader.GetDateTime(1);
                commande.Acompte = (double)reader.GetDecimal(3);
                commande.EstPrete = reader.GetBoolean(4);
                commande.EstRecuperee = reader.GetBoolean(5);
                commande.Total = (double)reader.GetDecimal(6);
                commande.Lignes = new List<LigneCommande>();
                commandes.Add(commande);
            }
            return commandes;
        }

        public static int Creer( Commande commande )
        {
            string sql = @"INSERT INTO commande 
                (client_id, categorie_evenement_id, date_creation, date_retrait,
                 acompte, est_prete, est_recuperee, total, nb_personne)
                VALUES (@client, @catEvt, CURRENT_DATE, @retrait,
                        @acompte, false, false, @total, @nbp)
                RETURNING commande_id";

            using var cmd = new NpgsqlCommand(sql, DALConnexion.GetConnexion());
            cmd.Parameters.AddWithValue("@client", commande.Client.ClientId);
            cmd.Parameters.AddWithValue("@catEvt",
                commande.CategorieEvenement != null
                    ? (object)commande.CategorieEvenement.CategorieEvenementId
                    : DBNull.Value);
            cmd.Parameters.AddWithValue("@retrait", commande.DateRetrait);
            cmd.Parameters.AddWithValue("@acompte", commande.Acompte);
            cmd.Parameters.AddWithValue("@total", commande.Total);
            cmd.Parameters.AddWithValue("@nbp", commande.NbPersonnes);

            int commandeId = (int)cmd.ExecuteScalar();

            // Insertion des lignes
            foreach(var ligne in commande.Lignes)
            {
                string sqlLigne = @"INSERT INTO ligne_commande 
                    (commande_id, produit_id, quantite, est_decoupe)
                    VALUES (@cmd, @prod, @qte, @dec)";
                using var cmdL = new NpgsqlCommand(sqlLigne, DALConnexion.GetConnexion());
                cmdL.Parameters.AddWithValue("@cmd", commandeId);
                cmdL.Parameters.AddWithValue("@prod", ligne.Produit.ProduitId);
                cmdL.Parameters.AddWithValue("@qte", ligne.Quantite);
                cmdL.Parameters.AddWithValue("@dec", ligne.EstDecoupe);
                cmdL.ExecuteNonQuery();
            }
            return commandeId;
        }

        public static void MarquerPrete( int commandeId )
        {
            string sql = "UPDATE commande SET est_prete = true WHERE commande_id = @id";
            using var cmd = new NpgsqlCommand(sql, DALConnexion.GetConnexion());
            cmd.Parameters.AddWithValue("@id", commandeId);
            cmd.ExecuteNonQuery();
        }

        public static void MarquerRecuperee( int commandeId )
        {
            string sql = "UPDATE commande SET est_recuperee = true WHERE commande_id = @id";
            using var cmd = new NpgsqlCommand(sql, DALConnexion.GetConnexion());
            cmd.Parameters.AddWithValue("@id", commandeId);
            cmd.ExecuteNonQuery();
        }
        public static void ModifierStatut( int commandeId, bool estPrete, bool estRecuperee )
        {
            string sql = @"UPDATE commande 
                   SET est_prete = @prete, est_recuperee = @recuperee
                   WHERE commande_id = @id";
            using var cmd = new NpgsqlCommand(sql, DALConnexion.GetConnexion());
            cmd.Parameters.AddWithValue("@prete", estPrete);
            cmd.Parameters.AddWithValue("@recuperee", estRecuperee);
            cmd.Parameters.AddWithValue("@id", commandeId);
            cmd.ExecuteNonQuery();
        }

        public static List<CategorieEvenement> GetCategoriesEvenement()
        {
            var categories = new List<CategorieEvenement>();
            string sql = "SELECT categorie_evenement_id, categorie_evenement_nom FROM categorie_evenement ORDER BY categorie_evenement_nom";
            using var cmd = new NpgsqlCommand(sql, DALConnexion.GetConnexion());
            using var reader = cmd.ExecuteReader();
            while(reader.Read())
                categories.Add(new CategorieEvenement(reader.GetInt32(0), reader.GetString(1)));
            return categories;
        }

        public static List<LigneCommande> GetLignesCommande( int commandeId )
        {
            var lignes = new List<LigneCommande>();
            string sql = @"
        SELECT lc.produit_id, lc.quantite, lc.est_decoupe,
               p.nb_parts, p.prix, p.est_indisponible,
               r.recette_id, r.recette_nom, r.recette_description,
               c.categorie_id, c.categorie_nom
        FROM ligne_commande lc
        JOIN produit p ON lc.produit_id = p.produit_id
        JOIN recette r ON p.recette_id = r.recette_id
        JOIN categorie c ON r.categorie_id = c.categorie_id
        WHERE lc.commande_id = @id";

            using var cmd = new NpgsqlCommand(sql, DALConnexion.GetConnexion());
            cmd.Parameters.AddWithValue("@id", commandeId);
            using var reader = cmd.ExecuteReader();
            while(reader.Read())
            {
                var cat = new Categorie(reader.GetInt32(9), reader.GetString(10));
                var rec = new Recette(reader.GetInt32(6), reader.GetString(7),
                              reader.IsDBNull(8) ? "" : reader.GetString(8), cat);
                var produit = new Produit(reader.GetInt32(0), rec,
                                  reader.GetInt32(3), (double)reader.GetDecimal(4),
                                  reader.GetBoolean(5));
                lignes.Add(new LigneCommande(produit, reader.GetInt32(1), reader.GetBoolean(2)));
            }
            return lignes;
        }

        public static void ModifierCommande( int commandeId, List<LigneCommande> lignes, double total )
        {
            // Supprimer les anciennes lignes
            string sqlDelete = "DELETE FROM ligne_commande WHERE commande_id = @id";
            using var cmdD = new NpgsqlCommand(sqlDelete, DALConnexion.GetConnexion());
            cmdD.Parameters.AddWithValue("@id", commandeId);
            cmdD.ExecuteNonQuery();

            // Mettre à jour le total et l'acompte
            string sqlUpdate = @"UPDATE commande 
                         SET total = @total, acompte = @acompte 
                         WHERE commande_id = @id";
            using var cmdU = new NpgsqlCommand(sqlUpdate, DALConnexion.GetConnexion());
            cmdU.Parameters.AddWithValue("@total", total);
            cmdU.Parameters.AddWithValue("@acompte", total * 0.25);
            cmdU.Parameters.AddWithValue("@id", commandeId);
            cmdU.ExecuteNonQuery();

            // Réinsérer les nouvelles lignes
            foreach(var ligne in lignes)
            {
                string sqlLigne = @"INSERT INTO ligne_commande 
                            (commande_id, produit_id, quantite, est_decoupe)
                            VALUES (@cmd, @prod, @qte, @dec)";
                using var cmdL = new NpgsqlCommand(sqlLigne, DALConnexion.GetConnexion());
                cmdL.Parameters.AddWithValue("@cmd", commandeId);
                cmdL.Parameters.AddWithValue("@prod", ligne.Produit.ProduitId);
                cmdL.Parameters.AddWithValue("@qte", ligne.Quantite);
                cmdL.Parameters.AddWithValue("@dec", ligne.EstDecoupe);
                cmdL.ExecuteNonQuery();
            }
        }

    }
}
