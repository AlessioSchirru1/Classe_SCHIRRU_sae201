using Classe_SAE201_bis.DAL;
using Classe_SAE201_bis.Metier;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Classe_SAE201_bis.Pages
{
	public partial class WindowAjouterProduit : Window
	{
		private List<Recette> _recettes;
		private List<Allergene> _tousAllergenes;

		public WindowAjouterProduit()
		{
			InitializeComponent();
			ChargerDonnees();
		}

		private void ChargerDonnees()
		{
			try
			{
				_recettes = DALRecette.GetToutes() ?? new List<Recette>();
				CbRecette.ItemsSource = _recettes;
				CbRecette.DisplayMemberPath = "RecetteNom";

				var categories = DALCategorie.GetToutes() ?? new List<Categorie>();
				CbCategorie.ItemsSource = categories;
				CbCategorie.DisplayMemberPath = "CategorieNom";

				_tousAllergenes = DALProduit.GetTousAllergenes() ?? new List<Allergene>();
				ListAllergenes.ItemsSource = _tousAllergenes
					.Select(a => new AllergeneCheck { Allergene = a })
					.ToList();
			}
			catch (System.Exception ex)
			{
				MessageBox.Show($"Erreur chargement : {ex.Message}");
			}
		}

		private void BtnEnregistrer_Click(object sender, RoutedEventArgs e)
		{
			if (CbRecette.SelectedItem is not Recette recette)
			{
				TxtErreur.Text = "Sélectionnez une recette.";
				TxtErreur.Visibility = Visibility.Visible;
				return;
			}
			if (!int.TryParse(TxtNbParts.Text, out int nbParts) || nbParts < 1)
			{
				TxtErreur.Text = "Nombre de parts invalide (min. 1).";
				TxtErreur.Visibility = Visibility.Visible;
				return;
			}
			if (!double.TryParse(TxtPrix.Text.Replace(",", "."),
				System.Globalization.NumberStyles.Any,
				System.Globalization.CultureInfo.InvariantCulture, out double prix) || prix < 0)
			{
				TxtErreur.Text = "Prix invalide.";
				TxtErreur.Visibility = Visibility.Visible;
				return;
			}

			try
			{
				var produit = new Produit(recette, nbParts, prix);
				DALProduit.Ajouter(produit);
				DialogResult = true;
				Close();
			}
			catch (System.Exception ex)
			{
				TxtErreur.Text = $"Erreur : {ex.Message}";
				TxtErreur.Visibility = Visibility.Visible;
			}
		}

		private void BtnNouvelleRecette_Click(object sender, RoutedEventArgs e)
		{
			PanelNouvelleRecette.Visibility =
				PanelNouvelleRecette.Visibility == Visibility.Collapsed
					? Visibility.Visible : Visibility.Collapsed;
		}

		private void BtnCreerRecette_Click(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrWhiteSpace(TxtNomRecette.Text) ||
				CbCategorie.SelectedItem is not Categorie cat)
			{
				TxtErreurRecette.Text = "Nom et catégorie obligatoires.";
				TxtErreurRecette.Visibility = Visibility.Visible;
				return;
			}

			try
			{
				var recette = new Recette(TxtNomRecette.Text.Trim(),
										  TxtDescRecette.Text.Trim(), cat);
				int recetteId = DALRecette.Ajouter(recette);
				recette.RecetteId = recetteId;

				var allergenesCoches = (ListAllergenes.ItemsSource as List<AllergeneCheck>)
					?.Where(a => a.Coche)
					.Select(a => a.Allergene)
					.ToList();

				if (allergenesCoches?.Count > 0)
					DALProduit.AjouterAllergenesRecette(recetteId, allergenesCoches);

				_recettes = DALRecette.GetToutes();
				CbRecette.ItemsSource = _recettes;
				CbRecette.SelectedItem = _recettes.FirstOrDefault(r => r.RecetteId == recetteId);
				PanelNouvelleRecette.Visibility = Visibility.Collapsed;
				TxtErreurRecette.Visibility = Visibility.Collapsed;
				TxtNomRecette.Text = "";
				TxtDescRecette.Text = "";
			}
			catch (System.Exception ex)
			{
				TxtErreurRecette.Text = $"Erreur : {ex.Message}";
				TxtErreurRecette.Visibility = Visibility.Visible;
			}
		}

		private void BtnAnnuler_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
			Close();
		}
	}

	public class AllergeneCheck
	{
		public Allergene Allergene { get; set; }
		public bool Coche { get; set; }
		public string Nom => Allergene.AllergeneNom;
	}
}