using EmissorNFe.ViewModel;
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
using System.Windows.Shapes;
using DgSystems.NFe.ViewModels;

namespace EmissorNFe.Imposto
{
    /// <summary>
    /// Interaction logic for CadastroImpostoWindow.xaml
    /// </summary>
    public partial class CadastroImpostoWindow : Window
    {
        public CadastroImpostoWindow()
        {
            this.DataContext = (Application.Current.Resources["Locator"] as ViewModelLocator).Imposto;

            InitializeComponent();
        }

        private void lblQtdeProduto_GotFocus(object sender, RoutedEventArgs e)
        {
            var txtBox = sender as TextBox;
            if (txtBox.Text == "0")
            {
                txtBox.Text = "";
            }
        }
    }
}
