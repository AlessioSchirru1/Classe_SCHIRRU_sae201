using Classe_SAE201_bis.DAL;
using Classe_SAE201_bis.Metier;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Classe_SAE201_bis.Pages
{
    public partial class PageGestionCommandes :Page
    {
        private Salarie _salarie;
        private List<CommandeVM> _toutesCommandes;

        public PageGestionCommandes( Salarie salarie )
        {
            InitializeComponent();
            _salarie = salarie;
            ChargerCommandes();
        }

        private void ChargerCommandes()
        {
            var commandes = DALCommande.GetToutes();
            _toutesCommandes = commandes.Select(c => new CommandeVM(c)).ToList();
            GridCommandes.ItemsSource = _toutesCommandes;
        }

        private void FiltreBtn_Click( object sender, RoutedEventArgs e )
        {
            var panel = (StackPanel)((Button)sender).Parent;
            foreach(var child in panel.Children)
                if(child is Button b)
                    b.Style = FindResource("FiltreBtnStyle") as Style;

            var btnClique = (Button)sender;
            btnClique.Style = FindResource("FiltreBtnActiveStyle") as Style;
            AppliquerFiltre(btnClique.Tag.ToString());
        }

        private void AppliquerFiltre( string filtre )
        {
            IEnumerable<CommandeVM> filtrees = _toutesCommandes;
            switch(filtre)
            {
                case "En attente":
                    filtrees = _toutesCommandes.Where(c => c.StatutSelectionne == "En attente");
                    break;
                case "Prêtes":
                    filtrees = _toutesCommandes.Where(c => c.StatutSelectionne == "Prête");
                    break;
                case "Restituées":
                    filtrees = _toutesCommandes.Where(c => c.StatutSelectionne == "Restituée");
                    break;
            }
            GridCommandes.ItemsSource = filtrees.ToList();
        }

        private void CbStatut_SelectionChanged( object sender, SelectionChangedEventArgs e )
        {
            if(e.AddedItems.Count == 0 || e.RemovedItems.Count == 0) return;

            var cb = (ComboBox)sender;
            int id = (int)cb.Tag;
            string nouveauStatut = cb.SelectedItem.ToString();

            bool estPrete = nouveauStatut == "Prête" || nouveauStatut == "Restituée";
            bool estRecuperee = nouveauStatut == "Restituée";

            DALCommande.ModifierStatut(id, estPrete, estRecuperee);

            // Mettre à jour le badge
            var vm = _toutesCommandes.FirstOrDefault(c => c.CommandeId == id);
            if(vm != null)
            {
                vm.StatutSelectionne = nouveauStatut;
                vm.MettreAJourBadge();
            }

            // Rafraîchir le DataGrid
            GridCommandes.Items.Refresh();
        }

        private void BtnModifier_Click(object sender, RoutedEventArgs e)
        {
            int commandeId = (int)((Button)sender).Tag;
            NavigationService?.Navigate(new PageModifierCommande(commandeId, _salarie));
        }
    }

    public class CommandeVM :INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public int CommandeId { get; set; }
        public string NomClient { get; set; }
        public string DateRetrait { get; set; }
        public int NbPersonnes { get; set; }
        public string TotalStr { get; set; }
        public List<string> StatutsPossibles { get; set; }

        private string _statutStr;
        public string StatutStr
        {
            get => _statutStr;
            set { _statutStr = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatutStr))); }
        }

        private Brush _couleurStatut;
        public Brush CouleurStatut
        {
            get => _couleurStatut;
            set { _couleurStatut = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CouleurStatut))); }
        }

        private string _statutSelectionne;
        public string StatutSelectionne
        {
            get => _statutSelectionne;
            set { _statutSelectionne = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatutSelectionne))); }
        }

        public CommandeVM( Commande c )
        {
            CommandeId = c.CommandeId;
            NomClient = $"{c.Client.Prenom} {c.Client.Nom}";
            DateRetrait = c.DateRetrait.ToString("dd MMM yyyy");
            NbPersonnes = c.NbPersonnes;
            TotalStr = $"{c.Total:0.00} €";
            StatutsPossibles = new List<string> { "En attente", "Prête", "Restituée" };

            if(c.EstRecuperee)
                StatutSelectionne = "Restituée";
            else if(c.EstPrete)
                StatutSelectionne = "Prête";
            else
                StatutSelectionne = "En attente";

            MettreAJourBadge();
        }

        public void MettreAJourBadge()
        {
            switch(StatutSelectionne)
            {
                case "Restituée":
                    StatutStr = "Restituée";
                    CouleurStatut = new SolidColorBrush(Color.FromRgb(150, 150, 150));
                    break;
                case "Prête":
                    StatutStr = "Prête";
                    CouleurStatut = new SolidColorBrush(Color.FromRgb(76, 175, 80));
                    break;
                default:
                    StatutStr = "En attente";
                    CouleurStatut = new SolidColorBrush(Color.FromRgb(255, 152, 0));
                    break;
            }
        }
        
    }
}
