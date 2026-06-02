using Classe_SAE201_bis.DAL;
using Classe_SAE201_bis.Metier;
using System.Windows;

namespace Classe_SAE201_bis.Pages
{
    public partial class WindowModifierProduit : Window
    {
        private Produit _produit;

        public WindowModifierProduit(Produit produit)
        {
            InitializeComponent();
            _produit = produit;
            TxtTitre.Text = $"Modifier : {produit.Recette.RecetteNom}";
            TxtPrix.Text = produit.Prix.ToString("0.00");
            ChkIndisponible.IsChecked = produit.EstIndisponible;
        }

        private void BtnEnregistrer_Click(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(TxtPrix.Text.Replace(",", "."),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out double prix) || prix < 0)
            {
                TxtErreur.Text = "Prix invalide.";
                TxtErreur.Visibility = Visibility.Visible;
                return;
            }
            _produit.Prix = prix;
            _produit.EstIndisponible = ChkIndisponible.IsChecked == true;
            DALProduit.Modifier(_produit);
            DialogResult = true;
            Close();
        }

        private void BtnAnnuler_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
