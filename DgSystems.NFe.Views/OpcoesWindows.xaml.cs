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

namespace EmissorNFe.View.Configurações
{
    /// <summary>
    /// Interaction logic for OpcoesWindows.xaml
    /// </summary>
    public partial class OpcoesWindows : Window
    {
        public OpcoesWindows()
        {
            this.DataContext = (Application.Current.Resources["Locator"] as ViewModelLocator).Opcoes;
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
