using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace classes_sae201.Metier
{
    internal class Recette
    {
        public int RecetteId { get; set; }
        public string RecetteNom { get; set; }
        public string RecetteDescription { get; set; }
        public Categorie Categorie { get; set; }
        public List<Allergene> Allergenes { get; set; }

        public Recette(string nom, string description, Categorie categorie)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            throw new NotImplementedException();
        }
    }
}
