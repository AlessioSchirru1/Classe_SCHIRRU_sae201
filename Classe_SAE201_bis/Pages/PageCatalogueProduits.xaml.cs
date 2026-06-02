using Classe_SAE201_bis.Metier;
using System.Windows.Controls;

namespace Classe_SAE201_bis.Pages
{
    public partial class PageCatalogueProduits :Page
    {
        private Salarie _salarie;
        public PageCatalogueProduits( Salarie salarie )
        {
            InitializeComponent();
            _salarie = salarie;
        }
    }
}
