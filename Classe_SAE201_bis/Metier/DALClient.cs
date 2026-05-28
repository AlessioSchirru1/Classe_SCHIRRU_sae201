using System.Collections.Generic;
using Npgsql;
using Classe_SAE201_bis.Metier;

namespace Classe_SAE201_bis.DAL
{
    public class DALClient
    {
        /// <summary>
        /// Recherche des clients par nom ou téléphone.
        /// </summary>
        public static List<Client> Rechercher( string recherche )
        {
            var clients = new List<Client>();
            string sql = @"SELECT client_id, nom, prenom, telephone, mail
                           FROM client
                           WHERE LOWER(nom) LIKE LOWER(@r)
                              OR LOWER(prenom) LIKE LOWER(@r)
                              OR telephone LIKE @r
                           ORDER BY nom, prenom";

            using var cmd = new NpgsqlCommand(sql, DALConnexion.GetConnexion());
            cmd.Parameters.AddWithValue("@r", $"%{recherche}%");

            using var reader = cmd.ExecuteReader();
            while(reader.Read())
            {
                clients.Add(new Client(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.IsDBNull(2) ? "" : reader.GetString(2),
                    reader.GetString(3),
                    reader.IsDBNull(4) ? "" : reader.GetString(4)
                ));
            }
            return clients;
        }

        /// <summary>
        /// Crée un nouveau client en base et retourne son ID.
        /// </summary>
        public static int Creer( Client client )
        {
            string sql = @"INSERT INTO client (nom, prenom, telephone, mail)
                           VALUES (@nom, @prenom, @tel, @mail)
                           RETURNING client_id";

            using var cmd = new NpgsqlCommand(sql, DALConnexion.GetConnexion());
            cmd.Parameters.AddWithValue("@nom", client.Nom);
            cmd.Parameters.AddWithValue("@prenom", client.Prenom ?? "");
            cmd.Parameters.AddWithValue("@tel", client.Telephone);
            cmd.Parameters.AddWithValue("@mail", client.Mail ?? "");

            return (int)cmd.ExecuteScalar();
        }
    }
}
