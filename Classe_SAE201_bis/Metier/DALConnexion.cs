using System;
using System.IO;
using System.Text.Json;
using Npgsql;

namespace Classe_SAE201_bis.DAL
{
    public class DALConnexion
    {
        private static NpgsqlConnection _connexion;

        private static string SERVEUR;
        private static string PORT;
        private static string BASE;

        static DALConnexion()
        {
            // Cherche config.json dans le dossier du projet
            string chemin = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
            string json = File.ReadAllText(chemin);
            var config = JsonDocument.Parse(json).RootElement;

            SERVEUR = config.GetProperty("serveur").GetString();
            PORT = config.GetProperty("port").GetString();
            BASE = config.GetProperty("base").GetString();
        }

        public static NpgsqlConnection OuvrirConnexion( string login, string motDePasse )
        {
            string chaineCnx = $"Host={SERVEUR};Port={PORT};Database={BASE};" +
                               $"Username={login};Password={motDePasse};";
            _connexion = new NpgsqlConnection(chaineCnx);
            _connexion.Open();
            return _connexion;
        }

        public static NpgsqlConnection GetConnexion()
        {
            if(_connexion == null || _connexion.State != System.Data.ConnectionState.Open)
                throw new Exception("Connexion non ouverte. Appelez OuvrirConnexion d'abord.");
            return _connexion;
        }

        public static void FermerConnexion()
        {
            if(_connexion != null && _connexion.State == System.Data.ConnectionState.Open)
            {
                _connexion.Close();
                _connexion = null;
            }
        }

        public static string GetCurrentUser()
        {
            using var cmd = new NpgsqlCommand("SELECT current_user", GetConnexion());
            return cmd.ExecuteScalar()?.ToString();
        }
    }
}