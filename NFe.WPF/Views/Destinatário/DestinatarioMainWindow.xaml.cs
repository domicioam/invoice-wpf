using EmissorNFe.View.Destinatario;
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

namespace EmissorNFe.View.Destinatário
{
    /// <summary>
    /// Interaction logic for DestinatarioMainWindow.xaml
    /// </summary>
    public partial class DestinatarioMainWindow : UserControl
    {
        public DestinatarioMainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var app = Application.Current;
            var mainWindow = app.MainWindow;
            new DestinatarioWindow() { Owner = mainWindow }.ShowDialog();
        }
    }
}
