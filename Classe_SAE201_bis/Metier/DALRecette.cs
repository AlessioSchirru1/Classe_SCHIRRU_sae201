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
				recettes.Add(new Recette(reader.GetInt32(0), reader.GetString(1),
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
				recettes.Add(new Recette(reader.GetInt32(0), reader.GetString(1),
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

		public static void Modifier(int recetteId, string nom, string description)
		{
			string sql = @"UPDATE recette SET recette_nom = @nom, recette_description = @desc
                           WHERE recette_id = @id";
			using var cmd = new NpgsqlCommand(sql, DALConnexion.GetConnexion());
			cmd.Parameters.AddWithValue("@nom", nom);
			cmd.Parameters.AddWithValue("@desc", description ?? "");
			cmd.Parameters.AddWithValue("@id", recetteId);
			cmd.ExecuteNonQuery();
		}

		public static void MettreAJourAllergenes(int recetteId, List<Allergene> allergenes)
		{
			// Supprimer tous les allergènes existants
			string sqlDelete = "DELETE FROM recette_allergene WHERE recette_id = @id";
			using var cmdD = new NpgsqlCommand(sqlDelete, DALConnexion.GetConnexion());
			cmdD.Parameters.AddWithValue("@id", recetteId);
			cmdD.ExecuteNonQuery();

			// Réinsérer les cochés
			foreach (var a in allergenes)
			{
				string sql = @"INSERT INTO recette_allergene (allergene_id, recette_id)
                               VALUES (@aid, @rid) ON CONFLICT DO NOTHING";
				using var cmd = new NpgsqlCommand(sql, DALConnexion.GetConnexion());
				cmd.Parameters.AddWithValue("@aid", a.AllergeneId);
				cmd.Parameters.AddWithValue("@rid", recetteId);
				cmd.ExecuteNonQuery();
			}
		}
	}
}