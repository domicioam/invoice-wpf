using EmissorNFe.View.Certificado;
using EmissorNFe.ViewModel;
using Microsoft.Win32;
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

namespace EmissorNFe.Certificado
{
    /// <summary>
    /// Interaction logic for AdicionarCertificadoWindow.xaml
    /// </summary>
    public partial class AdicionarCertificadoWindow : Window
    {
        public AdicionarCertificadoWindow()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            new SenhaDialogWindow() { Owner = this }.ShowDialog();
        }
    }
}
