using Classe_SAE201_bis.DAL;
using Classe_SAE201_bis.Metier;
using System.Windows;
using System.Windows.Controls;

namespace Classe_SAE201_bis
{
	public partial class WindowConnexion : Window
	{
		public WindowConnexion()
		{
			InitializeComponent();
		}

		private void BtnConnecter_Click(object sender, RoutedEventArgs e)
		{
			string login = TxtLogin.Text.Trim();
			string mdp = TxtPassword.Password;

			if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(mdp))
			{
				TxtErreur.Text = "Veuillez remplir tous les champs.";
				TxtErreur.Visibility = Visibility.Visible;
				return;
			}

			try
			{
				DALConnexion.OuvrirConnexion(login, mdp);
				Salarie salarie = DALSalarie.GetSalarieConnecte();

				MainWindow main = new MainWindow(salarie);
				main.Show();
				this.Close();
			}
			catch
			{
				TxtErreur.Text = "Identifiant ou mot de passe incorrect.";
				TxtErreur.Visibility = Visibility.Visible;
			}
		}

        private void TxtLogin_TextChanged( object sender, TextChangedEventArgs e )
        {
            PlaceholderLogin.Visibility = string.IsNullOrEmpty(TxtLogin.Text)
                ? Visibility.Visible : Visibility.Collapsed;
        }

        private void TxtPassword_PasswordChanged( object sender, RoutedEventArgs e )
        {
            PlaceholderPassword.Visibility = string.IsNullOrEmpty(TxtPassword.Password)
                ? Visibility.Visible : Visibility.Collapsed;
        }

        private void BtnFermer_Click( object sender, RoutedEventArgs e )
        {
            Application.Current.Shutdown();
        }
    }
}