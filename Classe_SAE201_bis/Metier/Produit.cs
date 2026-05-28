using Classe_SAE201_bis.Metier;
namespace Classe_SAE201_bis.Metier
{
    public class Produit
    {
        public int ProduitId { get; set; }
        public bool EstIndisponible { get; set; }
        public int NbParts { get; set; }
        public double Prix { get; set; }
        public Recette Recette { get; set; }

        public Produit( int id, Recette recette, int nbParts, double prix, bool estIndisponible = false )
        {
            ProduitId = id;
            Recette = recette;
            NbParts = nbParts;
            Prix = prix;
            EstIndisponible = estIndisponible;
        }

        public Produit( Recette recette, int nbParts, double prix )
            : this(0, recette, nbParts, prix) { }

        public override string ToString()
        {
            string categorie = Recette?.Categorie?.CategorieNom ?? "";
            if(categorie == "Gâteaux")
                return $"{Recette?.RecetteNom} — {NbParts} parts — {Prix:C}";
            else if(categorie == "Viennoiseries")
                return NbParts == 1
                    ? $"{Recette?.RecetteNom} — à l'unité — {Prix:C}"
                    : $"{Recette?.RecetteNom} — lot de {NbParts} — {Prix:C}";
            else
                return $"{Recette?.RecetteNom} — {Prix:C}";
        }
    }
}
