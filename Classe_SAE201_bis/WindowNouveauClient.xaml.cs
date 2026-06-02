using Classe_SAE201_bis.Metier;
using System.Windows;

namespace Classe_SAE201_bis
{
    public partial class WindowNouveauClient :Window
    {
        public Client NouveauClient { get; private set; }

        public WindowNouveauClient()
        {
            InitializeComponent();
        }

        private void BtnCreer_Click( object sender, RoutedEventArgs e )
        {
            if(string.IsNullOrWhiteSpace(TxtNom.Text) || string.IsNullOrWhiteSpace(TxtTelephone.Text))
            {
                TxtErreur.Text = "Le nom et le téléphone sont obligatoires.";
                TxtErreur.Visibility = Visibility.Visible;
                return;
            }

            NouveauClient = new Client(
                TxtNom.Text.Trim(),
                TxtPrenom.Text.Trim(),
                TxtTelephone.Text.Trim(),
                TxtMail.Text.Trim()
            );

            DialogResult = true;
            Close();
        }

        private void BtnAnnuler_Click( object sender, RoutedEventArgs e )
        {
            DialogResult = false;
            Close();
        }

        private void BtnFermer_Click( object sender, RoutedEventArgs e )
        {
            DialogResult = false;
            Close();
        }
    }
}
