using Classe_SAE201_bis.Metier;
using System.Windows;
using System.Windows.Controls;

namespace Classe_SAE201_bis
{
    public partial class MainWindow :Window
    {
        private Salarie _salarie;
        private Button _btnActif;

        public MainWindow( Salarie salarie )
        {
            InitializeComponent();
            _salarie = salarie;
            ChargerInfosSalarie();
            ChargerMenu();
        }

        private void ChargerInfosSalarie()
        {
            TxtNomSalarie.Text = $"{_salarie.Prenom} {_salarie.Nom}";
            TxtRoleSalarie.Text = _salarie.EstVendeur() ? "Vendeur" : "Chef boulanger";

            // Initiales
            string initiales = "";
            if(!string.IsNullOrEmpty(_salarie.Prenom)) initiales += _salarie.Prenom[0];
            if(!string.IsNullOrEmpty(_salarie.Nom)) initiales += _salarie.Nom[0];
            TxtInitiales.Text = initiales.ToUpper();
        }

        private void ChargerMenu()
        {
            PanelMenu.Children.Clear();

            if(_salarie.EstVendeur())
            {
                AjouterBoutonMenu("📋  Commandes du jour", () => NaviguerVers("CommandesDuJour"));
                AjouterBoutonMenu("➕  Nouvelle commande", () => NaviguerVers("NouvelleCommande"));
                AjouterBoutonMenu("👥  Clients", () => NaviguerVers("Clients"));
            }
            else
            {
                AjouterBoutonMenu("📋  Commandes", () => NaviguerVers("GestionCommandes"));
                AjouterBoutonMenu("🛍️  Produits", () => NaviguerVers("CatalogueProduits"));
                AjouterBoutonMenu("🗂️  Catégories", () => NaviguerVers("Categories"));
            }

            // Activer le premier bouton par défaut
            if(PanelMenu.Children.Count > 0)
            {
                ActiverBouton((Button)PanelMenu.Children[0]);
                string premierePage = _salarie.EstVendeur() ? "CommandesDuJour" : "GestionCommandes";
                NaviguerVers(premierePage);
            }
                
        }

        private void AjouterBoutonMenu( string libelle, System.Action action )
        {
            var btn = new Button
            {
                Content = libelle,
                Style = FindResource("MenuBtnStyle") as Style
            };
            btn.Click += ( s, e ) =>
            {
                ActiverBouton(btn);
                action();
            };
            PanelMenu.Children.Add(btn);
        }

        private void ActiverBouton( Button btn )
        {
            if(_btnActif != null)
                _btnActif.Style = FindResource("MenuBtnStyle") as Style;

            btn.Style = FindResource("MenuBtnActiveStyle") as Style;
            _btnActif = btn;
        }

        private void NaviguerVers( string page )
        {
            switch(page)
            {
                case "CommandesDuJour":
                    FrameContenu.Navigate(new Pages.PageCommandesDuJour(_salarie));
                    break;
                case "NouvelleCommande":
                    FrameContenu.Navigate(new Pages.PageNouvelleCommande(_salarie));
                    break;
                case "Clients":
                    FrameContenu.Navigate(new Pages.PageClients(_salarie));
                    break;
                case "GestionCommandes":
                    FrameContenu.Navigate(new Pages.PageGestionCommandes(_salarie));
                    break;
                case "CatalogueProduits":
                    FrameContenu.Navigate(new Pages.PageCatalogueProduits(_salarie));
                    break;
                case "Categories":
                    FrameContenu.Navigate(new Pages.PageCategories(_salarie));
                    break;
            }
        }

        private void BtnDeconnexion_Click( object sender, RoutedEventArgs e )
        {
            DAL.DALConnexion.FermerConnexion();
            var connexion = new WindowConnexion();
            connexion.Show();
            this.Close();
        }

        private void BtnFermer_Click( object sender, RoutedEventArgs e )
        {
            Application.Current.Shutdown();
        }

        private void BarreTitre_MouseDown( object sender, System.Windows.Input.MouseButtonEventArgs e )
        {
            if(e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
                this.DragMove();
        }
    }
}
