using Classe_SAE201_bis.DAL;
using Classe_SAE201_bis.Metier;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Classe_SAE201_bis.Pages
{
    public partial class PageClients :Page
    {
        private Salarie _salarie;
        private List<ClientVM> _tousClients;

        public PageClients( Salarie salarie )
        {
            InitializeComponent();
            _salarie = salarie;
            ChargerClients();
        }

        private void ChargerClients()
        {
            var clients = DALClient.Rechercher("");
            _tousClients = clients.Select(c => new ClientVM(c)).ToList();
            MettreAJourAffichage(_tousClients);
        }

        private void MettreAJourAffichage( List<ClientVM> liste )
        {
            TxtTotalClients.Text = _tousClients.Count.ToString();
            TxtAvecCommandes.Text = _tousClients.Count(c => c.NbCommandes > 0).ToString();
            TxtSansCommande.Text = _tousClients.Count(c => c.NbCommandes == 0).ToString();
            TxtNbClients.Text = $"Liste des clients ({liste.Count})";
            GridClients.ItemsSource = liste;
        }

        private void TxtRecherche_TextChanged( object sender, TextChangedEventArgs e )
        {
            PlaceholderRecherche.Visibility = string.IsNullOrEmpty(TxtRecherche.Text)
                ? Visibility.Visible : Visibility.Collapsed;

            string recherche = TxtRecherche.Text.ToLower();
            if(string.IsNullOrWhiteSpace(recherche))
            {
                MettreAJourAffichage(_tousClients);
                return;
            }

            var filtres = _tousClients.Where(c =>
                c.NomComplet.ToLower().Contains(recherche) ||
                c.Mail.ToLower().Contains(recherche) ||
                c.Telephone.Contains(recherche)
            ).ToList();

            TxtNbClients.Text = $"Liste des clients ({filtres.Count})";
            GridClients.ItemsSource = filtres;
        }

        private void BtnNouveauClient_Click( object sender, RoutedEventArgs e )
        {
            var dialog = new WindowNouveauClient();
            if(dialog.ShowDialog() == true)
            {
                DALClient.Creer(dialog.NouveauClient);
                ChargerClients();
            }
        }
    }

    public class ClientVM
    {
        public int ClientId { get; set; }
        public string IdStr { get; set; }
        public string NomComplet { get; set; }
        public string Mail { get; set; }
        public string Telephone { get; set; }
        public int NbCommandes { get; set; }

        public ClientVM( Client c )
        {
            ClientId = c.ClientId;
            IdStr = $"CLT-{c.ClientId:D3}";
            NomComplet = $"{c.Prenom} {c.Nom}".Trim();
            Mail = c.Mail ?? "—";
            Telephone = c.Telephone;
            NbCommandes = c.Commandes?.Count ?? 0;
        }
    }
}
