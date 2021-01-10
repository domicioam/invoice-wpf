using System.Windows;
using DgSystems.NFe.ViewModels;

namespace DgSystems.NFe.Views
{
    /// <summary>
    /// Interaction logic for SenhaDialogWindow.xaml
    /// </summary>
    public partial class SenhaDialogWindow : Window
    {
        public SenhaDialogWindow(CertificadoViewModel viewModel)
        {
            this.DataContext = viewModel;

            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void senha_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if(!string.IsNullOrWhiteSpace(senha.Password))
            {
                buttonSave.IsEnabled = true;
            }
        }
    }
}
