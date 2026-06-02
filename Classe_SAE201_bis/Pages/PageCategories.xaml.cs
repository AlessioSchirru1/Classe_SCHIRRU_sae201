using Classe_SAE201_bis.DAL;
using Classe_SAE201_bis.Metier;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Classe_SAE201_bis.Pages
{
    public partial class PageCategories : Page
    {
        private Salarie _salarie;
        private List<CategorieVM> _categories;
        private int _categorieEnCoursId = 0; // 0 = ajout, >0 = modification

        public PageCategories(Salarie salarie)
        {
            InitializeComponent();
            _salarie = salarie;
            ChargerCategories();
        }

        private void ChargerCategories()
        {
            var cats = DALCategorie.GetToutes();
            _categories = cats.Select(c =>
            {
                var recettes = DALRecette.GetParCategorie(c.CategorieId);
                return new CategorieVM(c, recettes);
            }).ToList();

            
            GridCategories.ItemsSource = _categories;
        }

        private void GridCategories_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GridCategories.SelectedItem is CategorieVM vm)
            {
                _categorieEnCoursId = vm.CategorieId;
                TxtTitrePanelDetail.Text = $"Modifier : {vm.Nom}";
                TxtNomCategorie.Text = vm.Nom;
                BtnAnnuler.Visibility = Visibility.Visible;
                SeparateurDetail.Visibility = Visibility.Visible;
                PanelDetail.Visibility = Visibility.Visible;
                TxtErreur.Visibility = Visibility.Collapsed;

                // Charger les recettes
                var recettes = DALRecette.GetParCategorie(vm.CategorieId);
                ListeRecettes.ItemsSource = recettes.Select(r => r.RecetteNom).ToList();
            }
        }

        private void BtnAjouter_Click(object sender, RoutedEventArgs e)
        {
            // Reset le panneau en mode ajout
            _categorieEnCoursId = 0;
            TxtTitrePanelDetail.Text = "Ajouter une catégorie";
            TxtNomCategorie.Text = "";
            TxtErreur.Visibility = Visibility.Collapsed;
            BtnAnnuler.Visibility = Visibility.Collapsed;
            SeparateurDetail.Visibility = Visibility.Collapsed;
            PanelDetail.Visibility = Visibility.Collapsed;
            GridCategories.SelectedItem = null;
            TxtNomCategorie.Focus();
        }

        private void BtnEnregistrer_Click(object sender, RoutedEventArgs e)
        {
            string nom = TxtNomCategorie.Text.Trim();
            if (string.IsNullOrWhiteSpace(nom))
            {
                TxtErreur.Text = "Le nom de la catégorie est obligatoire.";
                TxtErreur.Visibility = Visibility.Visible;
                return;
            }

            // Vérif doublon
            bool doublon = _categories.Any(c =>
                c.Nom.ToLower() == nom.ToLower() && c.CategorieId != _categorieEnCoursId);
            if (doublon)
            {
                TxtErreur.Text = "Une catégorie avec ce nom existe déjà.";
                TxtErreur.Visibility = Visibility.Visible;
                return;
            }

            if (_categorieEnCoursId == 0)
            {
                // Ajout
                DALCategorie.Ajouter(new Categorie(0, nom));
                MessageBox.Show($"Catégorie \"{nom}\" ajoutée avec succès !",
                    "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // Modification
                DALCategorie.Modifier(new Categorie(_categorieEnCoursId, nom));
                MessageBox.Show($"Catégorie \"{nom}\" modifiée avec succès !",
                    "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            TxtNomCategorie.Text = "";
            TxtErreur.Visibility = Visibility.Collapsed;
            _categorieEnCoursId = 0;
            TxtTitrePanelDetail.Text = "Ajouter une catégorie";
            BtnAnnuler.Visibility = Visibility.Collapsed;
            SeparateurDetail.Visibility = Visibility.Collapsed;
            PanelDetail.Visibility = Visibility.Collapsed;
            ChargerCategories();
        }

        private void BtnAnnuler_Click(object sender, RoutedEventArgs e)
        {
            _categorieEnCoursId = 0;
            TxtNomCategorie.Text = "";
            TxtErreur.Visibility = Visibility.Collapsed;
            TxtTitrePanelDetail.Text = "Ajouter une catégorie";
            BtnAnnuler.Visibility = Visibility.Collapsed;
            SeparateurDetail.Visibility = Visibility.Collapsed;
            PanelDetail.Visibility = Visibility.Collapsed;
            GridCategories.SelectedItem = null;
        }
    }

    public class CategorieVM
    {
        public int CategorieId { get; set; }
        public string IdStr { get; set; }
        public string Nom { get; set; }
        public int NbRecettes { get; set; }
        public int NbProduits { get; set; }

        public CategorieVM(Categorie c, List<Recette> recettes)
        {
            CategorieId = c.CategorieId;
            IdStr = $"CAT-{c.CategorieId:D2}";
            Nom = c.CategorieNom;
            NbRecettes = recettes.Count;
            NbProduits = recettes.Sum(r => r.Allergenes?.Count ?? 0); // approximation
        }
    }
}
