using System.Windows;
using Classe_SAE201_bis.Metier;

namespace Classe_SAE201_bis
{
    public partial class MainWindow :Window
    {
        private Salarie _salarie;

        public MainWindow( Salarie salarie )
        {
            InitializeComponent();
            _salarie = salarie;
        }
    }
}