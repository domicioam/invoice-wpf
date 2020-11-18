using NFe.WPF.ViewModel;
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

namespace EmissorNFe.View.Destinatario
{
    /// <summary>
    /// Interaction logic for DestinatarioWindow.xaml
    /// </summary>
    public partial class DestinatarioWindow : Window
    {
        public DestinatarioWindow()
        {
            Resources.Add("TextoExpander", "Endereço (Opcional NFC - e)");
            InitializeComponent();
        }

        public DestinatarioWindow(bool isNFe)
        {
            Resources.Add("IsNFE", isNFe);
            var textoExpander = isNFe ? "Endereço (Obrigatório NF-e)" : "Endereço (Opcional NFC - e)";
            Resources.Add("TextoExpander", textoExpander);
            InitializeComponent();
        }

        public DestinatarioWindow(DestinatarioViewModel viewModel)
        {
            bool isNFe = viewModel.DestinatarioParaSalvar.IsNFe;
            Resources.Add("IsNFE", isNFe);
            var textoExpander = isNFe ? "Endereço (Obrigatório NF-e)" : "Endereço (Opcional NFC - e)";
            Resources.Add("TextoExpander", textoExpander);

            this.DataContext = viewModel;
            InitializeComponent();
        }
    }
}
