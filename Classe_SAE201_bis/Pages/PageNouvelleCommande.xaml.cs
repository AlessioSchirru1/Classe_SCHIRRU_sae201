using Classe_SAE201_bis.DAL;
using Classe_SAE201_bis.Metier;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Classe_SAE201_bis.Pages
{
    public partial class PageNouvelleCommande :Page
    {
        private Salarie _salarie;
        private int _etape = 1;
        private List<ProduitCardVM> _tousProduitsVM;
        private List<LigneSelectionVM> _selection = new List<LigneSelectionVM>();
        private List<ClientListVM> _tousClients;
        private Client _clientSelectionne;
        private List<CategorieEvenement> _categories;

        public PageNouvelleCommande( Salarie salarie )
        {
            InitializeComponent();
            _salarie = salarie;
            ChargerProduits();
            ChargerClients();
            ChargerCategories();
            DpDateRetrait.SelectedDate = DateTime.Today.AddDays(3);
        }

        // ===================== CHARGEMENT =====================

        private void ChargerProduits()
        {
            var produits = DALProduit.GetTous(false);
            var groupes = produits.GroupBy(p => p.Recette.RecetteId);
            _tousProduitsVM = groupes.Select(g =>
            {
                var p0 = g.First();
                var allergenes = DALProduit.GetAllergenesPourRecette(p0.Recette.RecetteId);
                return new ProduitCardVM
                {
                    RecetteId = p0.Recette.RecetteId,
                    RecetteNom = p0.Recette.RecetteNom,
                    Description = p0.Recette.RecetteDescription,
                    Categorie = p0.Recette.Categorie.CategorieNom,
                    Allergenes = allergenes.Count > 0 ? string.Join(", ", allergenes.Select(a => a.AllergeneNom)) : "",
                    Formats = g.Select(p => new FormatVM { ProduitId = p.ProduitId, Libelle = FormatLibelle(p), Prix = p.Prix }).ToList(),
                    FormatSelectionne = null
                };
            }).ToList();
            ListeProduits.ItemsSource = _tousProduitsVM;
        }

        private string FormatLibelle( Produit p )
        {
            string cat = p.Recette.Categorie.CategorieNom;
            if(cat == "Gâteaux")
                return $"{p.NbParts} personnes — {p.Prix:0.00} €";
            else if(cat == "Viennoiseries" && p.NbParts == 1)
                return $"Unité — {p.Prix:0.00} €";
            else if(cat == "Viennoiseries")
                return $"Lot de {p.NbParts} — {p.Prix:0.00} €";
            else
                return p.NbParts == 1 ? $"Entier — {p.Prix:0.00} €" : $"Tranché — {p.Prix:0.00} €";
        }

        private void ChargerClients()
        {
            var clients = DALClient.Rechercher("");
            _tousClients = clients.Select(c => new ClientListVM(c)).ToList();
            ListeClients.ItemsSource = _tousClients;
        }

        private void ChargerCategories()
        {
            _categories = DALCommande.GetCategoriesEvenement();
            CbTypeEvenement.ItemsSource = _categories;
            CbTypeEvenement.DisplayMemberPath = "CategorieEvenementNom";
        }

        // ===================== ÉTAPE 1 : PRODUITS =====================

        private void TxtRechercheProduit_TextChanged( object sender, TextChangedEventArgs e )
        {
            PlaceholderProduit.Visibility = string.IsNullOrEmpty(TxtRechercheProduit.Text)
                ? Visibility.Visible : Visibility.Collapsed;
            AppliquerFiltreProduits();
        }

        private void FiltreCat_Click( object sender, RoutedEventArgs e )
        {
            var panel = (StackPanel)((Button)sender).Parent;
            foreach(var child in panel.Children)
                if(child is Button b) b.Style = FindResource("FiltreBtnStyle") as Style;
            ( (Button)sender ).Style = FindResource("FiltreBtnActiveStyle") as Style;
            AppliquerFiltreProduits();
        }

        private void AppliquerFiltreProduits()
        {
            string recherche = TxtRechercheProduit.Text.ToLower();
            string cat = "";
            var btns = new[] { BtnTous, BtnGateaux, BtnViennoiseries, BtnPains };
            foreach(var b in btns)
                if(b.Style == FindResource("FiltreBtnActiveStyle") as Style)
                    cat = b.Tag.ToString();

            var filtres = _tousProduitsVM.Where(p =>
                (cat == "Tous" || cat == "" || p.Categorie == cat) &&
                (string.IsNullOrEmpty(recherche) || p.RecetteNom.ToLower().Contains(recherche))
            ).ToList();
            ListeProduits.ItemsSource = filtres;
        }

        private void CbFormat_SelectionChanged( object sender, SelectionChangedEventArgs e )
        {
            if(sender is ComboBox cb && cb.SelectedItem is FormatVM fmt)
            {
                int recetteId = (int)cb.Tag;
                var vm = _tousProduitsVM.FirstOrDefault(p => p.RecetteId == recetteId);
                if(vm != null) vm.FormatSelectionne = fmt;
            }
        }

        private void BtnAjouter_Click( object sender, RoutedEventArgs e )
        {
            int recetteId = (int)((Button)sender).Tag;
            var vm = _tousProduitsVM.FirstOrDefault(p => p.RecetteId == recetteId);
            if(vm?.FormatSelectionne == null) return;

            var existant = _selection.FirstOrDefault(s => s.ProduitId == vm.FormatSelectionne.ProduitId);
            if(existant != null)
                existant.Quantite++;
            else
                _selection.Add(new LigneSelectionVM
                {
                    ProduitId = vm.FormatSelectionne.ProduitId,
                    Nom = vm.RecetteNom,
                    Format = vm.FormatSelectionne.Libelle,
                    Prix = vm.FormatSelectionne.Prix,
                    Quantite = 1
                });
            MettreAJourPanier();
        }

        private void BtnMoins_Click( object sender, RoutedEventArgs e )
        {
            int id = (int)((Button)sender).Tag;
            var ligne = _selection.FirstOrDefault(s => s.ProduitId == id);
            if(ligne == null) return;
            if(ligne.Quantite <= 1) _selection.Remove(ligne);
            else ligne.Quantite--;
            MettreAJourPanier();
        }

        private void BtnPlus_Click( object sender, RoutedEventArgs e )
        {
            int id = (int)((Button)sender).Tag;
            var ligne = _selection.FirstOrDefault(s => s.ProduitId == id);
            if(ligne != null) { ligne.Quantite++; MettreAJourPanier(); }
        }

        private void MettreAJourPanier()
        {
            ListeSelection.ItemsSource = null;
            ListeSelection.ItemsSource = _selection;
            TxtPanier0.Visibility = _selection.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            double total = _selection.Sum(s => s.SousTotal);
            TxtTotalHT.Text = $"{total:0.00} €";
            TxtAcompte.Text = $"{total * 0.25:0.00} €";
            TxtAcompte2.Text = $"{total * 0.25:0.00} €";
        }

        // ===================== ÉTAPE 2 : CLIENT =====================

        private void TxtRechercheClient_TextChanged( object sender, TextChangedEventArgs e )
        {
            PlaceholderClient.Visibility = string.IsNullOrEmpty(TxtRechercheClient.Text)
                ? Visibility.Visible : Visibility.Collapsed;
            string r = TxtRechercheClient.Text.ToLower();
            var filtres = string.IsNullOrWhiteSpace(r)
                ? _tousClients
                : _tousClients.Where(c => c.NomComplet.ToLower().Contains(r) || c.Telephone.Contains(r)).ToList();
            ListeClients.ItemsSource = filtres;
        }

        private void ClientItem_Click( object sender, System.Windows.Input.MouseButtonEventArgs e )
        {
            var border = (Border)sender;
            int id = (int)border.Tag;
            foreach(var c in _tousClients) c.EstSelectionne = Visibility.Collapsed;
            var vm = _tousClients.FirstOrDefault(c => c.ClientId == id);
            if(vm != null)
            {
                vm.EstSelectionne = Visibility.Visible;
                _clientSelectionne = new Client(vm.ClientId, vm.Nom, vm.Prenom, vm.Telephone, vm.Mail);
            }
            ListeClients.ItemsSource = null;
            ListeClients.ItemsSource = _tousClients;
            MettreAJourClientSelectionne();
        }

        private void BtnCreerNouveauClient_Click( object sender, RoutedEventArgs e )
        {
            PanelNouveauClient.Visibility = PanelNouveauClient.Visibility == Visibility.Visible
                ? Visibility.Collapsed : Visibility.Visible;
        }

        private void BtnValiderNouveauClient_Click( object sender, RoutedEventArgs e )
        {
            if(string.IsNullOrWhiteSpace(TxtNomNC.Text) || string.IsNullOrWhiteSpace(TxtTelNC.Text))
            {
                MessageBox.Show("Le nom et le téléphone sont obligatoires.");
                return;
            }
            try
            {
                var c = new Client(TxtNomNC.Text.Trim(), TxtPrenomNC.Text.Trim(),
                           TxtTelNC.Text.Trim(), TxtMailNC.Text.Trim());
                int id = DALClient.Creer(c);
                _clientSelectionne = new Client(id, c.Nom, c.Prenom, c.Telephone, c.Mail);
                ChargerClients();
                PanelNouveauClient.Visibility = Visibility.Collapsed;
                MettreAJourClientSelectionne();
            }
            catch(Exception)
            {
                MessageBox.Show("Ce téléphone existe déjà pour un autre client.",
                                "Doublon", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void MettreAJourClientSelectionne()
        {
            if(_clientSelectionne != null)
            {
                PanelClientSelectionne.Visibility = Visibility.Visible;
                TxtClientSelectionne.Text = $"{_clientSelectionne.Prenom} {_clientSelectionne.Nom} — {_clientSelectionne.Telephone}";
            }
        }

        private void CbTypeEvenement_SelectionChanged( object sender, SelectionChangedEventArgs e ) { }

        // ===================== NAVIGATION =====================

        private void BtnSuivant_Click( object sender, RoutedEventArgs e )
        {
            if(_etape == 1)
            {
                if(_selection.Count == 0) { MessageBox.Show("Veuillez ajouter au moins un produit."); return; }
                AllerEtape(2);
            }
            else if(_etape == 2)
            {
                if(_clientSelectionne == null) { MessageBox.Show("Veuillez sélectionner ou créer un client."); return; }
                if(DpDateRetrait.SelectedDate == null) { MessageBox.Show("Veuillez choisir une date de retrait."); return; }
                AllerEtape(3);
                RemplirRecap();
            }
            else if(_etape == 3)
            {
                ValiderCommande();
            }
        }

        private void BtnRetour_Click( object sender, RoutedEventArgs e )
        {
            AllerEtape(_etape - 1);
        }

        private void BtnAnnuler_Click( object sender, RoutedEventArgs e )
        {
            if(MessageBox.Show("Annuler la commande ?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                NavigationService?.GoBack();
        }

        private void AllerEtape( int etape )
        {
            _etape = etape;
            PanelEtape1.Visibility = etape == 1 ? Visibility.Visible : Visibility.Collapsed;
            PanelEtape2.Visibility = etape == 2 ? Visibility.Visible : Visibility.Collapsed;
            PanelEtape3.Visibility = etape == 3 ? Visibility.Visible : Visibility.Collapsed;
            BtnRetour.Visibility = etape > 1 ? Visibility.Visible : Visibility.Collapsed;
            BtnSuivant.Content = etape == 3 ? "✓ Valider la commande" : "Suivant →";

            MettreAJourIndicateurs();
        }

        private void MettreAJourIndicateurs()
        {
            var brun   = new SolidColorBrush(Color.FromRgb(107, 58, 42));
            var gris   = new SolidColorBrush(Color.FromRgb(212, 184, 154));
            var vert   = new SolidColorBrush(Color.FromRgb(76, 175, 80));

            Etape1Cercle.Background = _etape == 1 ? brun : vert;
            Etape2Cercle.Background = _etape == 2 ? brun : ( _etape > 2 ? vert : gris );
            Etape3Cercle.Background = _etape == 3 ? brun : gris;

            Etape1Txt.Foreground = _etape >= 1 ? brun : gris;
            Etape2Txt.Foreground = _etape >= 2 ? brun : gris;
            Etape3Txt.Foreground = _etape >= 3 ? brun : gris;
        }

        private void RemplirRecap()
        {
            RecapNom.Text = $"{_clientSelectionne.Prenom} {_clientSelectionne.Nom}";
            RecapTel.Text = _clientSelectionne.Telephone;
            RecapDate.Text = DpDateRetrait.SelectedDate?.ToString("dd MMMM yyyy",
                new System.Globalization.CultureInfo("fr-FR")) ?? "";
            RecapNbP.Text = TxtNbPersonnes.Text;
            RecapType.Text = ( CbTypeEvenement.SelectedItem as CategorieEvenement )?.CategorieEvenementNom ?? "—";

            GridRecap.ItemsSource = _selection;

            double total = _selection.Sum(s => s.SousTotal);
            RecapTotal.Text = $"{total:0.00} €";
            RecapAcompte.Text = $"{total * 0.25:0.00} €";
        }

        private void ValiderCommande()
        {
            try
            {
                double total   = _selection.Sum(s => s.SousTotal);
                double acompte = total * 0.25;

                var commande = new Commande(
                    0,
                    DpDateRetrait.SelectedDate ?? DateTime.Today.AddDays(3),
                    int.TryParse(TxtNbPersonnes.Text, out int nb) ? nb : 0,
                    _clientSelectionne,
                    CbTypeEvenement.SelectedItem as CategorieEvenement
                );
                commande.Total = total;
                commande.Acompte = acompte;

                var tousP = DALProduit.GetTous(false);
                foreach(var s in _selection)
                {
                    var produit = tousP.FirstOrDefault(p => p.ProduitId == s.ProduitId);
                    if(produit != null)
                        commande.Lignes.Add(new LigneCommande(produit, s.Quantite, false));
                }

                DALCommande.Creer(commande);
                NavigationService?.GoBack();
            }
            catch(Exception ex)
            {
                TxtErreurRecap.Text = $"Erreur : {ex.Message}";
                TxtErreurRecap.Visibility = Visibility.Visible;
            }
        }
    }

    // ===================== VIEW MODELS =====================

    public class ProduitCardVM
    {
        public int RecetteId { get; set; }
        public string RecetteNom { get; set; }
        public string Description { get; set; }
        public string Categorie { get; set; }
        public string Allergenes { get; set; }
        public List<FormatVM> Formats { get; set; }
        public FormatVM FormatSelectionne { get; set; }
        public bool PeutAjouter => true;
        public Brush CouleurAjouter => new SolidColorBrush(Color.FromRgb(107, 58, 42));
    }

    public class FormatVM
    {
        public int ProduitId { get; set; }
        public string Libelle { get; set; }
        public double Prix { get; set; }
    }

    public class LigneSelectionVM
    {
        public int ProduitId { get; set; }
        public string Nom { get; set; }
        public string Format { get; set; }
        public double Prix { get; set; }
        private int _quantite;
        public int Quantite
        {
            get => _quantite;
            set { _quantite = value; }
        }
        public double SousTotal => Prix * Quantite;
        public string SousTotalStr => $"{SousTotal:0.00} €";
        public string PrixStr => $"{Prix:0.00} €";
    }

    public class ClientListVM
    {
        public int ClientId { get; set; }
        public string Nom { get; set; }
        public string Prenom { get; set; }
        public string NomComplet { get; set; }
        public string Telephone { get; set; }
        public string Mail { get; set; }
        public Visibility EstSelectionne { get; set; } = Visibility.Collapsed;

        public ClientListVM( Client c )
        {
            ClientId = c.ClientId;
            Nom = c.Nom;
            Prenom = c.Prenom ?? "";
            NomComplet = $"{c.Prenom} {c.Nom}".Trim();
            Telephone = c.Telephone;
            Mail = c.Mail ?? "";
        }
    }
}
