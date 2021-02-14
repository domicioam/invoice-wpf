using EmissorNFe.ViewModel;
using NFe.WPF.ViewModel.Services;
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

namespace EmissorNFe.Produto
{
    /// <summary>
    /// Interaction logic for CadastroProdutoWindow.xaml
    /// </summary>
    public partial class CadastroProdutoWindow : Window, IClosable
    {
        public CadastroProdutoWindow()
        {
            this.DataContext = (Application.Current.Resources["Locator"] as ViewModelLocator).Produto;
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
