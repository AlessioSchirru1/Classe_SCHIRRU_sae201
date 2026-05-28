using Classe_SAE201_bis.Metier;
namespace Classe_SAE201_bis.Metier
{
    public class LigneCommande
    {
        public int Quantite { get; set; }
        public bool EstDecoupe { get; set; }
        public Produit Produit { get; set; }

        public LigneCommande( Produit produit, int quantite, bool estDecoupe = false )
        {
            Produit = produit;
            Quantite = quantite;
            EstDecoupe = estDecoupe;
        }

        public double CalculerSousTotal()
        {
            return Produit.Prix * Quantite;
        }

        public override string ToString()
        {
            return $"{Produit} x{Quantite} = {CalculerSousTotal():C}";
        }
    }
}
