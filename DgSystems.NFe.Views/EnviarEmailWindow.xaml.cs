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

namespace NFe.WPF.View.NotaFiscal
{
   /// <summary>
   /// Interaction logic for EnviarEmailWindow.xaml
   /// </summary>
   public partial class EnviarEmailWindow : Window, IClosable
   {
        public EnviarEmailWindow()
        {
            this.DataContext = (Application.Current.Resources["Locator"] as ViewModelLocator).EnviarEmail;
            InitializeComponent();
        }
        public EnviarEmailWindow(ViewModel.EnviarEmailViewModel enviarEmailViewModel)
      {
         InitializeComponent();
         DataContext = enviarEmailViewModel;
      }
   }
}
