namespace Classe_SAE201_bis.Metier
{
    public class CategorieEvenement
    {
        public int CategorieEvenementId { get; set; }
        public string CategorieEvenementNom { get; set; }

        public CategorieEvenement( int id, string nom )
        {
            CategorieEvenementId = id;
            CategorieEvenementNom = nom;
        }

        public override string ToString()
        {
            return CategorieEvenementNom;
        }
    }
}
