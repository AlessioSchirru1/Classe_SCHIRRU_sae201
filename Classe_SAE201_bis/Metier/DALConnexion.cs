using System;
using Npgsql;

namespace Classe_SAE201_bis.DAL
{
    /// <summary>
    /// Gère la connexion PostgreSQL.
    /// Le mot de passe est géré par PostgreSQL — on ne le stocke pas.
    /// La connexion se fait avec le login/mdp de l'employé.
    /// </summary>
    public class DALConnexion
    {
        private static NpgsqlConnection _connexion;

        // À adapter selon votre serveur IUT
        private const string SERVEUR = "srv-peda-new";
        private const string PORT    = "5433";
        private const string BASE    = "ikadarni_paul";

        /// <summary>
        /// Ouvre une connexion avec le login et mot de passe PostgreSQL de l'employé.
        /// </summary>
        public static NpgsqlConnection OuvrirConnexion( string login, string motDePasse )
        {
            string chaineCnx = $"Host={SERVEUR};Port={PORT};Database={BASE};" +
                               $"Username={login};Password={motDePasse};";
            _connexion = new NpgsqlConnection(chaineCnx);
            _connexion.Open();
            return _connexion;
        }

        /// <summary>
        /// Retourne la connexion déjà ouverte.
        /// </summary>
        public static NpgsqlConnection GetConnexion()
        {
            if(_connexion == null || _connexion.State != System.Data.ConnectionState.Open)
                throw new Exception("Connexion non ouverte. Appelez OuvrirConnexion d'abord.");
            return _connexion;
        }

        /// <summary>
        /// Ferme la connexion proprement.
        /// </summary>
        public static void FermerConnexion()
        {
            if(_connexion != null && _connexion.State == System.Data.ConnectionState.Open)
            {
                _connexion.Close();
                _connexion = null;
            }
        }

        /// <summary>
        /// Récupère le login PostgreSQL de l'utilisateur connecté (current_user).
        /// </summary>
        public static string GetCurrentUser()
        {
            using var cmd = new NpgsqlCommand("SELECT current_user", GetConnexion());
            return cmd.ExecuteScalar()?.ToString();
        }
    }
}
