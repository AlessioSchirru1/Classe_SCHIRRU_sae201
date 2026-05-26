using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace classes_sae201.Metier
{
    internal class LigneCommande
    {
        public int Quantite { get; set; }
        public bool EstDecoupe { get; set; }
        public Produit Produit { get; set; }

        public LigneCommande(Produit produit, int quantite)
        {
            throw new NotImplementedException();
        }

        public double CalculerSousTotal()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            throw new NotImplementedException();
        }
    }
}
