using Classe_SAE201_bis.DAL;
using Classe_SAE201_bis.Metier;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Navigation;

namespace Classe_SAE201_bis.Pages
{
    public partial class PageCommandesDuJour :Page
    {
        private Salarie _salarie;

        public PageCommandesDuJour( Salarie salarie )
        {
            InitializeComponent();
            _salarie = salarie;
            TxtDate.Text = DateTime.Now.ToString("dddd dd MMMM yyyy",
                new System.Globalization.CultureInfo("fr-FR"));
            ChargerCommandes();
        }

        private void ChargerCommandes()
        {
            var commandes = DALCommande.GetCommandesDuJour();
            var vms = commandes.Select(c => new CommandeDuJourVM(c)).ToList();

            // Stats
            TxtTotal.Text = vms.Count.ToString();
            TxtARetirer.Text = vms.Count(c => !c.EstRecuperee).ToString();
            TxtPretes.Text = vms.Count(c => c.EstPrete && !c.EstRecuperee).ToString();
            TxtRestituees.Text = vms.Count(c => c.EstRecuperee).ToString();

            GridCommandes.ItemsSource = vms;
        }

        private void BtnRestituer_Click( object sender, RoutedEventArgs e )
        {
            int id = (int)((Button)sender).Tag;
            DALCommande.MarquerRecuperee(id);
            ChargerCommandes();
        }

        private void BtnModifier_Click( object sender, RoutedEventArgs e )
        {
            int id = (int)((Button)sender).Tag;
            NavigationService?.Navigate(new PageModifierCommande(id, _salarie));
        }
    }

    public class CommandeDuJourVM
    {
        public int CommandeId { get; set; }
        public string NomClient { get; set; }
        public string Produits { get; set; }
        public string TotalStr { get; set; }
        public string StatutStr { get; set; }
        public Brush CouleurStatut { get; set; }
        public bool EstPrete { get; set; }
        public bool EstRecuperee { get; set; }
        public Visibility ShowBtnRestituer { get; set; }

        public CommandeDuJourVM( Commande c )
        {
            CommandeId = c.CommandeId;
            NomClient = $"{c.Client.Prenom} {c.Client.Nom}";
            TotalStr = $"{c.Total:0.00} €";
            EstPrete = c.EstPrete;
            EstRecuperee = c.EstRecuperee;

            Produits = c.Lignes.Count > 0
                ? string.Join(", ", c.Lignes.Select(l => l.Produit.Recette.RecetteNom))
                : "—";

            if(c.EstRecuperee)
            {
                StatutStr = "Restituée";
                CouleurStatut = new SolidColorBrush(Color.FromRgb(150, 150, 150));
                ShowBtnRestituer = Visibility.Collapsed;
            }
            else if(c.EstPrete)
            {
                StatutStr = "Prête";
                CouleurStatut = new SolidColorBrush(Color.FromRgb(76, 175, 80));
                ShowBtnRestituer = Visibility.Visible;
            }
            else
            {
                StatutStr = "En attente";
                CouleurStatut = new SolidColorBrush(Color.FromRgb(255, 152, 0));
                ShowBtnRestituer = Visibility.Collapsed;
            }
        }

        
    }
}
