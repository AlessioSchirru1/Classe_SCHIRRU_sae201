using System.Collections.Generic;
using Npgsql;

namespace Classe_SAE201_bis.DAL
{
    public class DALMenu
    {
        /// <summary>
        /// Retourne les items de menu selon le rôle du salarié connecté.
        /// </summary>
        public static List<string> GetMenuParRole( string role )
        {
            var menus = new List<string>();
            string sql = @"SELECT menu_libelle FROM menu
                           WHERE menu_role = @role
                           ORDER BY menu_ordre";

            using var cmd = new NpgsqlCommand(sql, DALConnexion.GetConnexion());
            cmd.Parameters.AddWithValue("@role", role);

            using var reader = cmd.ExecuteReader();
            while(reader.Read())
                menus.Add(reader.GetString(0));

            return menus;
        }
    }
}
