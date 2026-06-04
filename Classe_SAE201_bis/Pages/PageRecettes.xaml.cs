using Classe_SAE201_bis.DAL;
using Classe_SAE201_bis.Metier;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Classe_SAE201_bis.Pages
{
	public partial class PageRecettes : Page
	{
		private Salarie _salarie;
		private List<RecetteVM> _toutesRecettes;
		private RecetteVM _recetteSelectionnee;
		private string _filtreCategorie = "Toutes";

		public PageRecettes(Salarie salarie)
		{
			InitializeComponent();
			_salarie = salarie;
			ChargerRecettes();
		}

		private void ChargerRecettes()
		{
			var recettes = DALRecette.GetToutes();
			_toutesRecettes = recettes.Select(r =>
			{
				var allergenes = DALProduit.GetAllergenesPourRecette(r.RecetteId);
				var produits = DALProduit.GetTous(true)
					.Where(p => p.Recette.RecetteId == r.RecetteId).ToList();
				return new RecetteVM(r, allergenes, produits);
			}).ToList();
			AppliquerFiltre();
		}

		private void AppliquerFiltre()
		{
			var filtres = _filtreCategorie == "Toutes"
				? _toutesRecettes
				: _toutesRecettes.Where(r => r.Categorie == _filtreCategorie).ToList();
			GridRecettes.ItemsSource = filtres;
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

		private void GridRecettes_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (GridRecettes.SelectedItem is RecetteVM vm)
			{
				_recetteSelectionnee = vm;
				AfficherFiche(vm);
			}
		}

		private void AfficherFiche(RecetteVM vm)
		{
			PanelAucune.Visibility = Visibility.Collapsed;
			PanelFiche.Visibility = Visibility.Visible;
			PanelEdition.Visibility = Visibility.Collapsed;

			TxtTitreFiche.Text = vm.Nom;
			TxtCategorieFiche.Text = vm.Categorie;
			TxtDescFiche.Text = string.IsNullOrWhiteSpace(vm.Description)
				? "Aucune description." : vm.Description;

			PanelAllergenes.Children.Clear();
			if (vm.Allergenes.Count == 0)
			{
				PanelAllergenes.Children.Add(new TextBlock
				{
					Text = "Aucun allergène",
					FontSize = 12,
					Foreground = new SolidColorBrush(Color.FromRgb(136, 136, 136))
				});
			}
			else
			{
				foreach (var a in vm.Allergenes)
				{
					var badge = new Border
					{
						Background = new SolidColorBrush(Color.FromRgb(253, 246, 236)),
						CornerRadius = new CornerRadius(12),
						Padding = new Thickness(10, 4, 10, 4),
						Margin = new Thickness(0, 0, 6, 6)
					};
					badge.Child = new TextBlock
					{
						Text = a.AllergeneNom,
						FontSize = 11,
						Foreground = new SolidColorBrush(Color.FromRgb(107, 58, 42))
					};
					PanelAllergenes.Children.Add(badge);
				}
			}

			ListeFormats.ItemsSource = vm.Formats;
		}

		private void BtnEditer_Click(object sender, RoutedEventArgs e)
		{
			if (_recetteSelectionnee == null) return;
			PanelEdition.Visibility = Visibility.Visible;
			TxtEditNom.Text = _recetteSelectionnee.Nom;
			TxtEditDesc.Text = _recetteSelectionnee.Description;

			var tousAllergenes = DALProduit.GetTousAllergenes();
			ListEditAllergenes.ItemsSource = tousAllergenes.Select(a => new AllergeneCheck
			{
				Allergene = a,
				Coche = _recetteSelectionnee.Allergenes.Any(x => x.AllergeneId == a.AllergeneId)
			}).ToList();
		}

		private void BtnAnnulerEdit_Click(object sender, RoutedEventArgs e)
		{
			PanelEdition.Visibility = Visibility.Collapsed;
			TxtErreurEdit.Visibility = Visibility.Collapsed;
		}

		private void BtnSauvegarderEdit_Click(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrWhiteSpace(TxtEditNom.Text))
			{
				TxtErreurEdit.Text = "Le nom est obligatoire.";
				TxtErreurEdit.Visibility = Visibility.Visible;
				return;
			}
			try
			{
				DALRecette.Modifier(_recetteSelectionnee.RecetteId,
					TxtEditNom.Text.Trim(), TxtEditDesc.Text.Trim());

				var coches = (ListEditAllergenes.ItemsSource as List<AllergeneCheck>)
					?.Where(a => a.Coche).Select(a => a.Allergene).ToList()
					?? new List<Allergene>();
				DALRecette.MettreAJourAllergenes(_recetteSelectionnee.RecetteId, coches);

				PanelEdition.Visibility = Visibility.Collapsed;
				ChargerRecettes();
				var updated = _toutesRecettes
					.FirstOrDefault(r => r.RecetteId == _recetteSelectionnee.RecetteId);
				if (updated != null) AfficherFiche(updated);
			}
			catch (System.Exception ex)
			{
				TxtErreurEdit.Text = $"Erreur : {ex.Message}";
				TxtErreurEdit.Visibility = Visibility.Visible;
			}
		}

		private void BtnAjouterFormat_Click(object sender, RoutedEventArgs e)
		{
			if (_recetteSelectionnee == null) return;
			var recette = DALRecette.GetToutes()
				.FirstOrDefault(r => r.RecetteId == _recetteSelectionnee.RecetteId);
			if (recette == null) return;

			var dialog = new WindowAjouterFormat(recette);
			dialog.Owner = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
			if (dialog.ShowDialog() == true)
			{
				ChargerRecettes();
				var updated = _toutesRecettes
					.FirstOrDefault(r => r.RecetteId == _recetteSelectionnee.RecetteId);
				if (updated != null) AfficherFiche(updated);
			}
		}

		private void BtnModifierPrix_Click(object sender, RoutedEventArgs e)
		{
			int produitId = (int)((Button)sender).Tag;
			var produit = DALProduit.GetTous(true).FirstOrDefault(p => p.ProduitId == produitId);
			if (produit == null) return;

			var dialog = new WindowModifierProduit(produit);
			dialog.Owner = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
			if (dialog.ShowDialog() == true)
			{
				ChargerRecettes();
				var updated = _toutesRecettes
					.FirstOrDefault(r => r.RecetteId == _recetteSelectionnee.RecetteId);
				if (updated != null) AfficherFiche(updated);
			}
		}

		private void BtnSupprimerFormat_Click(object sender, RoutedEventArgs e)
		{
			int produitId = (int)((Button)sender).Tag;
			var result = MessageBox.Show("Supprimer ce format ?", "Confirmation",
				MessageBoxButton.YesNo, MessageBoxImage.Warning);
			if (result == MessageBoxResult.Yes)
			{
				try
				{
					DALProduit.Supprimer(produitId);
					ChargerRecettes();
					var updated = _toutesRecettes
						.FirstOrDefault(r => r.RecetteId == _recetteSelectionnee?.RecetteId);
					if (updated != null) AfficherFiche(updated);
				}
				catch
				{
					MessageBox.Show("Impossible de supprimer ce format (utilisé dans des commandes).",
						"Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
		}
	}

	public class RecetteVM
	{
		public int RecetteId { get; set; }
		public string Nom { get; set; }
		public string Categorie { get; set; }
		public string Description { get; set; }
		public string AllergenesStr { get; set; }
		public string NbFormats { get; set; }
		public List<Allergene> Allergenes { get; set; }
		public List<FormatPrixVM> Formats { get; set; }

		public RecetteVM(Recette r, List<Allergene> allergenes, List<Produit> produits)
		{
			RecetteId = r.RecetteId;
			Nom = r.RecetteNom;
			Categorie = r.Categorie.CategorieNom;
			Description = r.RecetteDescription;
			Allergenes = allergenes;
			AllergenesStr = allergenes.Count > 0
				? string.Join(", ", allergenes.Select(a => a.AllergeneNom))
				: "Aucun";
			NbFormats = produits.Count.ToString();
			Formats = produits.Select(p => new FormatPrixVM(p)).ToList();
		}
	}

	public class FormatPrixVM
	{
		public int ProduitId { get; set; }
		public string FormatStr { get; set; }
		public string PrixStr { get; set; }

		public FormatPrixVM(Produit p)
		{
			ProduitId = p.ProduitId;
			string cat = p.Recette.Categorie.CategorieNom;
			if (cat == "Viennoiseries")
				FormatStr = p.NbParts == 1 ? "Unité" : $"Lot de {p.NbParts}";
			else if (cat == "Pains")
				FormatStr = p.NbParts == 1 ? "Entier" : $"x{p.NbParts}";
			else
				FormatStr = p.NbParts == 1 ? "1 part" : $"{p.NbParts} parts";
			PrixStr = $"{p.Prix:0.00} €";
		}
	}
}