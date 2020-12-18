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

namespace EmissorNFe.View.Certificado
{
    /// <summary>
    /// Interaction logic for SenhaDialogWindow.xaml
    /// </summary>
    public partial class SenhaDialogWindow : Window
    {
        public SenhaDialogWindow()
        {
            this.DataContext = (Application.Current.Resources["Locator"] as ViewModelLocator).Certificado;

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
