using Classe_SAE201_bis.Metier;
using Npgsql;
using System.Collections.Generic;

namespace Classe_SAE201_bis.DAL
{
	public class DALCategorie
	{
		public static List<Categorie> GetToutes()
		{
			var categories = new List<Categorie>();
			string sql = "SELECT categorie_id, categorie_nom FROM categorie ORDER BY categorie_nom";
			using var cmd = new NpgsqlCommand(sql, DALConnexion.GetConnexion());
			using var reader = cmd.ExecuteReader();
			while (reader.Read())
				categories.Add(new Categorie(reader.GetInt32(0), reader.GetString(1)));
			return categories;
		}

		public static void Ajouter(Categorie categorie)
		{
			string sql = "INSERT INTO categorie (categorie_nom) VALUES (@nom)";
			using var cmd = new NpgsqlCommand(sql, DALConnexion.GetConnexion());
			cmd.Parameters.AddWithValue("@nom", categorie.CategorieNom);
			cmd.ExecuteNonQuery();
		}

		public static void Modifier(Categorie categorie)
		{
			string sql = "UPDATE categorie SET categorie_nom = @nom WHERE categorie_id = @id";
			using var cmd = new NpgsqlCommand(sql, DALConnexion.GetConnexion());
			cmd.Parameters.AddWithValue("@nom", categorie.CategorieNom);
			cmd.Parameters.AddWithValue("@id", categorie.CategorieId);
			cmd.ExecuteNonQuery();
		}

		public static void Supprimer(int categorieId)
		{
			string sql = "DELETE FROM categorie WHERE categorie_id = @id";
			using var cmd = new NpgsqlCommand(sql, DALConnexion.GetConnexion());
			cmd.Parameters.AddWithValue("@id", categorieId);
			cmd.ExecuteNonQuery();
		}
	}
}