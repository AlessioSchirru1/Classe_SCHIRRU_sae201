using System;

namespace Classe_SAE201_bis.Metier
{
    public class Allergene
    {
        public int AllergeneId { get; set; }
        public string AllergeneNom { get; set; }

        public Allergene( int id, string nom )
        {
            AllergeneId = id;
            AllergeneNom = nom;
        }

        public override string ToString()
        {
            return AllergeneNom;
        }
    }
}
