namespace Classe_SAE201_bis.Metier
{
    public class Categorie
    {
        public int CategorieId { get; set; }
        public string CategorieNom { get; set; }

        public Categorie( int id, string nom )
        {
            CategorieId = id;
            CategorieNom = nom;
        }

        public override string ToString()
        {
            return CategorieNom;
        }
    }
}
