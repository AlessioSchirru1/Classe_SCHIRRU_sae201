using Classe_SAE201_bis.DAL;
using Classe_SAE201_bis.Metier;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Classe_SAE201_bis.Pages
{
    public partial class PageModifierCommande :Page
    {
        private Salarie _salarie;
        private int _commandeId;
        private List<ProduitCardVM> _tousProduitsVM;
        private List<LigneSelectionVM> _selection = new List<LigneSelectionVM>();

        public PageModifierCommande( int commandeId, Salarie salarie )
        {
            InitializeComponent();
            _commandeId = commandeId;
            _salarie = salarie;
            ChargerCommande();
            ChargerProduits();
        }

        private void ChargerCommande()
        {
            var commandes = DALCommande.GetToutes();
            var commande = commandes.FirstOrDefault(c => c.CommandeId == _commandeId);
            if(commande != null)
            {
                TxtTitre.Text = "Modifier la commande";
                TxtSousTitre.Text = $"{commande.Client.Prenom} {commande.Client.Nom} — Retrait le {commande.DateRetrait:dd/MM/yyyy}";
            }

            var lignes = DALCommande.GetLignesCommande(_commandeId);
            _selection = lignes.Select(l => new LigneSelectionVM
            {
                ProduitId = l.Produit.ProduitId,
                Nom = l.Produit.Recette.RecetteNom,
                Format = FormatLibelle(l.Produit),
                Prix = l.Produit.Prix,
                Quantite = l.Quantite
            }).ToList();
            MettreAJourPanier();
        }

        private void ChargerProduits()
        {
            var produits = DALProduit.GetTous(false);
            var groupes  = produits.GroupBy(p => p.Recette.RecetteId);
            _tousProduitsVM = groupes.Select(g =>
            {
                var p0 = g.First();
                var allergenes = DALProduit.GetAllergenesPourRecette(p0.Recette.RecetteId);
                return new ProduitCardVM
                {
                    RecetteId = p0.Recette.RecetteId,
                    RecetteNom = p0.Recette.RecetteNom,
                    Categorie = p0.Recette.Categorie.CategorieNom,
                    Allergenes = allergenes.Count > 0 ? string.Join(", ", allergenes.Select(a => a.AllergeneNom)) : "",
                    Formats = g.Select(p => new FormatVM { ProduitId = p.ProduitId, Libelle = FormatLibelle(p), Prix = p.Prix }).ToList()
                };
            }).ToList();
            ListeProduits.ItemsSource = _tousProduitsVM;
        }

        private string FormatLibelle( Produit p )
        {
            string cat = p.Recette.Categorie.CategorieNom;
            if(cat == "Gâteaux") return $"{p.NbParts} personnes — {p.Prix:0.00} €";
            else if(cat == "Viennoiseries" && p.NbParts == 1) return $"Unité — {p.Prix:0.00} €";
            else if(cat == "Viennoiseries") return $"Lot de {p.NbParts} — {p.Prix:0.00} €";
            else return p.NbParts == 1 ? $"Entier — {p.Prix:0.00} €" : $"Tranché — {p.Prix:0.00} €";
        }

        private void TxtRechercheProduit_TextChanged( object sender, TextChangedEventArgs e )
        {
            PlaceholderProduit.Visibility = string.IsNullOrEmpty(TxtRechercheProduit.Text)
                ? Visibility.Visible : Visibility.Collapsed;
            AppliquerFiltre();
        }

        private void FiltreCat_Click( object sender, RoutedEventArgs e )
        {
            var panel = (StackPanel)((Button)sender).Parent;
            foreach(var child in panel.Children)
                if(child is Button b) b.Style = FindResource("FiltreBtnStyle") as Style;
            ( (Button)sender ).Style = FindResource("FiltreBtnActiveStyle") as Style;
            AppliquerFiltre();
        }

        private void AppliquerFiltre()
        {
            string recherche = TxtRechercheProduit.Text.ToLower();
            string cat = "";
            var btns = new[] { BtnTous, BtnGateaux, BtnViennoiseries, BtnPains };
            foreach(var b in btns)
                if(b.Style == FindResource("FiltreBtnActiveStyle") as Style)
                    cat = b.Tag.ToString();

            ListeProduits.ItemsSource = _tousProduitsVM.Where(p =>
                ( cat == "Tous" || cat == "" || p.Categorie == cat ) &&
                ( string.IsNullOrEmpty(recherche) || p.RecetteNom.ToLower().Contains(recherche) )
            ).ToList();
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
            if(existant != null) existant.Quantite++;
            else _selection.Add(new LigneSelectionVM
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
            TxtTotal.Text = $"{total:0.00} €";
            TxtAcompte.Text = $"{total * 0.25:0.00} €";
        }

        private void BtnEnregistrer_Click( object sender, RoutedEventArgs e )
        {
            if(_selection.Count == 0)
            {
                MessageBox.Show("La commande doit contenir au moins un produit.");
                return;
            }

            var tousP = DALProduit.GetTous(false);
            var lignes = _selection.Select(s =>
            {
                var produit = tousP.FirstOrDefault(p => p.ProduitId == s.ProduitId);
                return new LigneCommande(produit, s.Quantite, false);
            }).ToList();

            double total = _selection.Sum(s => s.SousTotal);
            DALCommande.ModifierCommande(_commandeId, lignes, total);

            NavigationService?.GoBack();
        }

        private void BtnRetour_Click( object sender, RoutedEventArgs e )
        {
            NavigationService?.GoBack();
        }
    }
}
