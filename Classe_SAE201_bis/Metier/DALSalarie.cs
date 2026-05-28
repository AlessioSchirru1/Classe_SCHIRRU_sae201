using System;
using Npgsql;
using Classe_SAE201_bis.Metier;

namespace Classe_SAE201_bis.DAL
{
    public class DALSalarie
    {
        /// <summary>
        /// Récupère le salarié connecté via current_user PostgreSQL.
        /// Utilisé juste après la connexion pour charger le rôle.
        /// </summary>
        public static Salarie GetSalarieConnecte()
        {
            string login = DALConnexion.GetCurrentUser();
            string sql = @"SELECT salarie_id, nom, prenom, login_salarie, role_salarie
                           FROM salarie
                           WHERE login_salarie = @login";

            using var cmd = new NpgsqlCommand(sql, DALConnexion.GetConnexion());
            cmd.Parameters.AddWithValue("@login", login);

            using var reader = cmd.ExecuteReader();
            if(reader.Read())
            {
                return new Salarie(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.GetString(3),
                    reader.GetString(4)
                );
            }
            throw new Exception($"Salarié '{login}' introuvable dans la base.");
        }
    }
}
