using System.Collections.Generic;

namespace Classe_SAE201_bis.Metier
{
    public class Recette
    {
        public int RecetteId { get; set; }
        public string RecetteNom { get; set; }
        public string RecetteDescription { get; set; }
        public Categorie Categorie { get; set; }
        public List<Allergene> Allergenes { get; set; }

        public Recette( int id, string nom, string description, Categorie categorie )
        {
            RecetteId = id;
            RecetteNom = nom;
            RecetteDescription = description;
            Categorie = categorie;
            Allergenes = new List<Allergene>();
        }

        public Recette( string nom, string description, Categorie categorie )
            : this(0, nom, description, categorie) { }

        public override string ToString()
        {
            return RecetteNom;
        }
    }
}
