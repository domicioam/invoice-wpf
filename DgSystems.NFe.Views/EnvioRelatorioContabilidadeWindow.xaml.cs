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

namespace EmissorNFe.View.Gerencial
{
    /// <summary>
    /// Interaction logic for EnvioRelatorioContabilidadeWindow.xaml
    /// </summary>
    public partial class EnvioRelatorioContabilidadeWindow : Window
    {
        public EnvioRelatorioContabilidadeWindow()
        {
            this.DataContext = (Application.Current.Resources["Locator"] as ViewModelLocator).EnviarContabilidade;

            InitializeComponent();
        }
    }
}
