using EmissorNFe.View;
using EmissorNFe.View.Destinatario;
using EmissorNFe.View.NotaFiscal;
using EmissorNFe.ViewModel;
using NFe.WPF.ViewModel.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EmissorNFe.NotaFiscal
{
    /// <summary>
    /// Interaction logic for NotaFiscalForm.xaml
    /// </summary>
    public partial class NFCeWindow : Window, IClosable
    {
        private string _oldValue;

        public NFCeWindow()
        {
            this.DataContext = (Application.Current.Resources["Locator"] as ViewModelLocator).NFCe;
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

        private void dataGrid1_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
        }

        private void novoDestinatario_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            new DestinatarioWindow() { Owner = this }.ShowDialog();
        }

        private void lblQtdeProduto_TextChanged(object sender, TextChangedEventArgs e)
        {
            var txtBox = sender as TextBox;

            if (Regex.IsMatch(txtBox.Text, "[^0-9]+"))
            {
                txtBox.Text = _oldValue;
                txtBox.CaretIndex = txtBox.Text.Length;
            }
            else
            {
                _oldValue = txtBox.Text;
            }
        }
    }
}
