using Classe_SAE201_bis.Metier;
using Classe_SAE201_bis.Metier;
using Npgsql;
using System;
using System.Collections.Generic;

namespace Classe_SAE201_bis.DAL
{
    public class DALCommande
    {
        /// <summary>
        /// Retourne les commandes dont la date de retrait est aujourd'hui.
        /// </summary>
        public static List<Commande> GetCommandesDuJour()
        {
            return GetCommandes("WHERE c.date_retrait = CURRENT_DATE");
        }

        /// <summary>
        /// Retourne toutes les commandes (pour le chef).
        /// </summary>
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

        /// <summary>
        /// Crée une nouvelle commande et retourne son ID.
        /// </summary>
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

        /// <summary>
        /// Marque une commande comme prête à emporter.
        /// </summary>
        public static void MarquerPrete( int commandeId )
        {
            string sql = "UPDATE commande SET est_prete = true WHERE commande_id = @id";
            using var cmd = new NpgsqlCommand(sql, DALConnexion.GetConnexion());
            cmd.Parameters.AddWithValue("@id", commandeId);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Marque une commande comme récupérée (restituée au client).
        /// </summary>
        public static void MarquerRecuperee( int commandeId )
        {
            string sql = "UPDATE commande SET est_recuperee = true WHERE commande_id = @id";
            using var cmd = new NpgsqlCommand(sql, DALConnexion.GetConnexion());
            cmd.Parameters.AddWithValue("@id", commandeId);
            cmd.ExecuteNonQuery();
        }
    }
}
