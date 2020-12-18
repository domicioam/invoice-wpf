using EmissorNFe.View.Destinatario;
using EmissorNFe.View.Transportadora;
using EmissorNFe.ViewModel;
using NFe.WPF.ViewModel.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EmissorNFe.View.NotaFiscal
{
    /// <summary>
    /// Interaction logic for NFeWindow.xaml
    /// </summary>
    public partial class NFeWindow : Window, IClosable
    {
        private string _oldValue;
        public bool HasSelectedValue { get; set; }

        public NFeWindow()
        {
            this.DataContext = (Application.Current.Resources["Locator"] as ViewModelLocator).NFe;
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
            new DestinatarioWindow(true) { Owner = this }.ShowDialog();
        }

        private void novaTransportadora_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            new AdicionarTransportadoraWindow() { Owner = this }.ShowDialog();
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

        private void comboBoxTransportadora_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            HasSelectedValue = ((ComboBox)sender).SelectedIndex != -1;
        }
    }
}
