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
    }
}

// Extension de DALProduit à ajouter dans DALProduit.cs :
/*
public static List<Allergene> GetTousAllergenes()
{
    var allergenes = new List<Allergene>();
    string sql = "SELECT allergene_id, allergene_nom FROM allergene ORDER BY allergene_nom";
    using var cmd = new NpgsqlCommand(sql, DALConnexion.GetConnexion());
    using var reader = cmd.ExecuteReader();
    while (reader.Read())
        allergenes.Add(new Allergene(reader.GetInt32(0), reader.GetString(1)));
    return allergenes;
}

public static void AjouterAllergenesRecette(int recetteId, List<Allergene> allergenes)
{
    foreach (var a in allergenes)
    {
        string sql = "INSERT INTO recette_allergene (allergene_id, recette_id) VALUES (@aid, @rid) ON CONFLICT DO NOTHING";
        using var cmd = new NpgsqlCommand(sql, DALConnexion.GetConnexion());
        cmd.Parameters.AddWithValue("@aid", a.AllergeneId);
        cmd.Parameters.AddWithValue("@rid", recetteId);
        cmd.ExecuteNonQuery();
    }
}
*/
