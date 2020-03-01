using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EmissorNFe.Imposto
{
    /// <summary>
    /// Interaction logic for ImpostoMainWindow.xaml
    /// </summary>
    public partial class ImpostoMainWindow : UserControl
    {
        public ImpostoMainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var app = Application.Current;
            var mainWindow = app.MainWindow;

            new CadastroImpostoWindow() { Owner = mainWindow }.ShowDialog();
        }
    }
}
