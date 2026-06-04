using Classe_SAE201_bis.DAL;
using Classe_SAE201_bis.Metier;
using System.Windows;

namespace Classe_SAE201_bis.Pages
{
    public partial class WindowAjouterFormat : Window
    {
        private Recette _recette;

        public WindowAjouterFormat(Recette recette)
        {
            InitializeComponent();
            _recette = recette;
            TxtTitre.Text = $"Nouveau format : {recette.RecetteNom}";
        }

        private void BtnAjouter_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(TxtNbParts.Text, out int nbParts) || nbParts < 1)
            {
                TxtErreur.Text = "Nombre de parts invalide (min. 1).";
                TxtErreur.Visibility = Visibility.Visible;
                return;
            }
            if (!double.TryParse(TxtPrix.Text.Replace(",", "."),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out double prix) || prix < 0)
            {
                TxtErreur.Text = "Prix invalide.";
                TxtErreur.Visibility = Visibility.Visible;
                return;
            }

            try
            {
                var produit = new Produit(_recette, nbParts, prix);
                DALProduit.Ajouter(produit);
                DialogResult = true;
                Close();
            }
            catch (System.Exception ex)
            {
                TxtErreur.Text = $"Erreur : {ex.Message}";
                TxtErreur.Visibility = Visibility.Visible;
            }
        }

        private void BtnAnnuler_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
