using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace classes_sae201.Metier
{
    internal class Produit
    {
        public int ProduitId { get; set; }
        public bool EstIndisponible { get; set; }
        public int NbParts { get; set; }
        public double Prix { get; set; }
        public Recette Recette { get; set; }

        public Produit(Recette recette, int nbParts, double prix)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            throw new NotImplementedException();
        }
    }
}
