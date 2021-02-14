using DgSystems.NFe.ViewModels;
using NFe.WPF.ViewModel;
using System.Windows;
using DgSystems.NFe.Views;

namespace EmissorNFe.Certificado
{
    /// <summary>
    /// Interaction logic for AdicionarCertificadoWindow.xaml
    /// </summary>
    public partial class AdicionarCertificadoWindow : Window
    {
        public AdicionarCertificadoWindow()
        {
            this.DataContext = (Application.Current.Resources["Locator"] as ViewModelLocator).Certificado;
            InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            new SenhaDialogWindow((CertificadoViewModel)DataContext) { Owner = this }.ShowDialog();
        }
    }
}
