using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace classes_sae201.Metier
{
    internal class Commande
    {
        public int CommandeId { get; set; }
        public DateTime DateCreation { get; set; }
        public DateTime DateRetrait { get; set; }
        public double Acompte { get; set; }
        public bool EstPrete { get; set; }
        public bool EstRecuperee { get; set; }
        public double Total { get; set; }
        public int NbPersonnes { get; set; }
        public Client Client { get; set; }
        public CategorieEvenement CategorieEvenement { get; set; }
        public List<LigneCommande> Lignes { get; set; }

        public Commande(DateTime dateRetrait, int nbPersonnes, Client client)
        {
            throw new NotImplementedException();
        }

        public double CalculerTotal()
        {
            throw new NotImplementedException();
        }

        public void MarquerPrete()
        {
            throw new NotImplementedException();
        }

        public void MarquerRecuperee()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            throw new NotImplementedException();
        }
    }
}
