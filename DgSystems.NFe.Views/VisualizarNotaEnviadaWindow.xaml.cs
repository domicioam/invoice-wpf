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
using DgSystems.NFe.ViewModels;

namespace EmissorNFe.View.NotaFiscal
{
    /// <summary>
    /// Interaction logic for VisualizarNotaEnviadaWindow.xaml
    /// </summary>
    public partial class VisualizarNotaEnviadaWindow : Window
    {
        public VisualizarNotaEnviadaWindow()
        {
            this.DataContext = (Application.Current.Resources["Locator"] as ViewModelLocator).VisualizarNotaEnviada;
            InitializeComponent();
        }

        public VisualizarNotaEnviadaWindow(NFe.WPF.ViewModel.VisualizarNotaEnviadaViewModel visualizarNotaEnviadaViewModel)
        {
            InitializeComponent();
            DataContext = visualizarNotaEnviadaViewModel;
        }
    }
}
