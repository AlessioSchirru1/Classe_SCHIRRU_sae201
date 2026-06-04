using Classe_SAE201_bis.DAL;
using Classe_SAE201_bis.Metier;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Classe_SAE201_bis.Pages
{
	public partial class PageCatalogueProduits : Page
	{
		private Salarie _salarie;
		private List<ProduitVM> _tousProduits;
		private string _filtreCategorie = "Tous";

		public PageCatalogueProduits(Salarie salarie)
		{
			InitializeComponent();
			_salarie = salarie;
			ChargerProduits();
		}

		private void ChargerProduits()
		{
			bool inclureIndisponibles = ChkIndisponibles?.IsChecked == true;
			var produits = DALProduit.GetTous(inclureIndisponibles);

			_tousProduits = produits.Select(p =>
			{
				var allergenes = DALProduit.GetAllergenesPourRecette(p.Recette.RecetteId);
				return new ProduitVM(p, allergenes);
			}).ToList();

			AppliquerFiltre();
		}

		private void AppliquerFiltre()
		{
			var filtres = _filtreCategorie == "Tous"
				? _tousProduits
				: _tousProduits.Where(p => p.CategorieNom == _filtreCategorie).ToList();
			GridProduits.ItemsSource = filtres;
		}

		private void FiltreBtn_Click(object sender, RoutedEventArgs e)
		{
			var panel = (StackPanel)((Button)sender).Parent;
			foreach (var child in panel.Children)
				if (child is Button b)
					b.Style = FindResource("FiltreBtnStyle") as Style;
			var btn = (Button)sender;
			btn.Style = FindResource("FiltreBtnActiveStyle") as Style;
			_filtreCategorie = btn.Tag.ToString();
			AppliquerFiltre();
		}

		private void ChkIndisponibles_Changed(object sender, RoutedEventArgs e)
		{
			ChargerProduits();
		}

		private void BtnModifier_Click(object sender, RoutedEventArgs e)
		{
			int id = (int)((Button)sender).Tag;
			var vm = _tousProduits.FirstOrDefault(p => p.ProduitId == id);
			if (vm == null) return;

			var dialog = new WindowModifierProduit(vm.ToProduit());
			if (dialog.ShowDialog() == true)
				ChargerProduits();
		}

		private void BtnToggleDisponible_Click(object sender, RoutedEventArgs e)
		{
			int id = (int)((Button)sender).Tag;
			var vm = _tousProduits.FirstOrDefault(p => p.ProduitId == id);
			if (vm == null) return;

			var produit = vm.ToProduit();
			produit.EstIndisponible = !produit.EstIndisponible;
			DALProduit.Modifier(produit);
			ChargerProduits();
		}

		private void BtnSupprimer_Click(object sender, RoutedEventArgs e)
		{
			int id = (int)((Button)sender).Tag;
			var result = MessageBox.Show(
				"Supprimer définitivement ce produit ?",
				"Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);

			if (result == MessageBoxResult.Yes)
			{
				try
				{
					DALProduit.Supprimer(id);
					ChargerProduits();
				}
				catch
				{
					MessageBox.Show(
						"Impossible de supprimer ce produit car il est utilisé dans des commandes.",
						"Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
		}

		private void BtnAjouterProduit_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				var dialog = new WindowAjouterProduit();
				if (dialog.ShowDialog() == true)
					ChargerProduits();
			}
			catch (System.Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}
	}

	public class ProduitVM
	{
		public int ProduitId { get; set; }
		public string RecetteNom { get; set; }
		public string CategorieNom { get; set; }
		public string FormatStr { get; set; }
		public string PrixStr { get; set; }
		public string AllergenesStr { get; set; }
		public string StatutStr { get; set; }
		public Brush BgStatut { get; set; }
		public Brush FgStatut { get; set; }
		public string BtnDispoLabel { get; set; }
		public Brush BgBtnDispo { get; set; }
		private Produit _produit;

		public ProduitVM(Produit p, List<Allergene> allergenes)
		{
			_produit = p;
			ProduitId = p.ProduitId;
			RecetteNom = p.Recette.RecetteNom;
			CategorieNom = p.Recette.Categorie.CategorieNom;
			PrixStr = $"{p.Prix:0.00} €";
			AllergenesStr = allergenes.Count > 0
				? string.Join(", ", allergenes.Select(a => a.AllergeneNom))
				: "Aucun";

			// Format selon catégorie
			if (CategorieNom == "Viennoiseries")
				FormatStr = p.NbParts == 1 ? "Unité" : $"Lot de {p.NbParts}";
			else if (CategorieNom == "Pains")
				FormatStr = p.NbParts == 1 ? "Entier" : $"x{p.NbParts}";
			else
				FormatStr = p.NbParts == 1 ? "1 part" : $"{p.NbParts} parts";

			// Statut
			if (p.EstIndisponible)
			{
				StatutStr = "Indisponible";
				BgStatut = new SolidColorBrush(Color.FromRgb(245, 230, 220));
				FgStatut = new SolidColorBrush(Color.FromRgb(180, 80, 50));
				BtnDispoLabel = "Remettre dispo";
				BgBtnDispo = new SolidColorBrush(Color.FromRgb(76, 175, 80));
			}
			else
			{
				StatutStr = "Disponible";
				BgStatut = new SolidColorBrush(Color.FromRgb(220, 242, 220));
				FgStatut = new SolidColorBrush(Color.FromRgb(50, 140, 50));
				BtnDispoLabel = "Désactiver";
				BgBtnDispo = new SolidColorBrush(Color.FromRgb(200, 100, 60));
			}
		}

		public Produit ToProduit() => _produit;
	}
}