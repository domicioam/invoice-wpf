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
using EmissorNFe.ViewModel;
using NFe.WPF.ViewModel;

namespace EmissorNFe.View.NotaFiscal
{
    /// <summary>
    /// Interaction logic for CancelarNotaWindow.xaml
    /// </summary>
    public partial class CancelarNotaWindow : Window
    {
        public CancelarNotaWindow()
        {
            this.DataContext = (Application.Current.Resources["Locator"] as ViewModelLocator).CancelarNota;
            InitializeComponent();
        }

        public CancelarNotaWindow(CancelarNotaViewModel cancelarNotaViewModel)
        {
            this.DataContext = cancelarNotaViewModel;
            InitializeComponent();
        }
    }
}
