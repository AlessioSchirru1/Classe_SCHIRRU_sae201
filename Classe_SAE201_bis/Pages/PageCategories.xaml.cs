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
		private int _categorieEnCoursId = 0;

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
				BtnSupprimer.Visibility = Visibility.Visible;
				SeparateurDetail.Visibility = Visibility.Visible;
				PanelDetail.Visibility = Visibility.Visible;
				TxtErreur.Visibility = Visibility.Collapsed;

				var recettes = DALRecette.GetParCategorie(vm.CategorieId);
				ListeRecettes.ItemsSource = recettes.Select(r => r.RecetteNom).ToList();
			}
		}

		private void BtnAjouter_Click(object sender, RoutedEventArgs e)
		{
			_categorieEnCoursId = 0;
			TxtTitrePanelDetail.Text = "Ajouter une catégorie";
			TxtNomCategorie.Text = "";
			TxtErreur.Visibility = Visibility.Collapsed;
			BtnAnnuler.Visibility = Visibility.Collapsed;
			BtnSupprimer.Visibility = Visibility.Collapsed;
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

			bool doublon = _categories.Any(c =>
				c.Nom.ToLower() == nom.ToLower() && c.CategorieId != _categorieEnCoursId);
			if (doublon)
			{
				TxtErreur.Text = "Une catégorie avec ce nom existe déjà.";
				TxtErreur.Visibility = Visibility.Visible;
				return;
			}

			try
			{
				if (_categorieEnCoursId == 0)
				{
					DALCategorie.Ajouter(new Categorie(0, nom));
					MessageBox.Show($"Catégorie \"{nom}\" ajoutée !",
						"Succès", MessageBoxButton.OK, MessageBoxImage.Information);
				}
				else
				{
					DALCategorie.Modifier(new Categorie(_categorieEnCoursId, nom));
					MessageBox.Show($"Catégorie \"{nom}\" modifiée !",
						"Succès", MessageBoxButton.OK, MessageBoxImage.Information);
				}

				ResetPanel();
				ChargerCategories();
			}
			catch (System.Exception ex)
			{
				TxtErreur.Text = $"Erreur : {ex.Message}";
				TxtErreur.Visibility = Visibility.Visible;
			}
		}

		private void BtnSupprimer_Click(object sender, RoutedEventArgs e)
		{
			if (_categorieEnCoursId == 0) return;

			var result = MessageBox.Show(
				$"Supprimer la catégorie \"{TxtNomCategorie.Text}\" ?\nCette action est irréversible.",
				"Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);

			if (result == MessageBoxResult.Yes)
			{
				try
				{
					DALCategorie.Supprimer(_categorieEnCoursId);
					ResetPanel();
					ChargerCategories();
				}
				catch
				{
					MessageBox.Show(
						"Impossible de supprimer cette catégorie car elle contient des recettes.",
						"Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
		}

		private void BtnAnnuler_Click(object sender, RoutedEventArgs e)
		{
			ResetPanel();
		}

		private void ResetPanel()
		{
			_categorieEnCoursId = 0;
			TxtNomCategorie.Text = "";
			TxtErreur.Visibility = Visibility.Collapsed;
			TxtTitrePanelDetail.Text = "Ajouter une catégorie";
			BtnAnnuler.Visibility = Visibility.Collapsed;
			BtnSupprimer.Visibility = Visibility.Collapsed;
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

		public CategorieVM(Categorie c, List<Recette> recettes)
		{
			CategorieId = c.CategorieId;
			IdStr = $"CAT-{c.CategorieId:D2}";
			Nom = c.CategorieNom;
			NbRecettes = recettes.Count;
		}
	}
}