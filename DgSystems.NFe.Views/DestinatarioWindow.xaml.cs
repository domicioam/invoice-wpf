using System.Windows;

using EmissorNFe.ViewModel;
using NFe.WPF.ViewModel;

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
            this.DataContext = (Application.Current.Resources["Locator"] as ViewModelLocator).Destinatario;
            InitializeComponent();
        }

        public DestinatarioWindow(bool isNFe)
        {
            Resources.Add("IsNFE", isNFe);
            Resources.Add("IsExpanded", isNFe);
            var textoExpander = isNFe ? "Endereço (Obrigatório NF-e)" : "Endereço (Opcional NFC - e)";
            Resources.Add("TextoExpander", textoExpander);
            this.DataContext = (Application.Current.Resources["Locator"] as ViewModelLocator).Destinatario;
            InitializeComponent();
        }

        public DestinatarioWindow(DestinatarioViewModel viewModel)
        {
            bool isNFe = viewModel.DestinatarioParaSalvar.IsNFe;
            Resources.Add("IsNFE", isNFe);
            var textoExpander = isNFe ? "Endereço (Obrigatório NF-e)" : "Endereço (Opcional NFC - e)";
            Resources.Add("TextoExpander", textoExpander);
            if(!string.IsNullOrWhiteSpace(viewModel.DestinatarioParaSalvar.Endereco.UF))
            {
                Resources.Add("IsExpanded", true);
            }

            this.DataContext = viewModel;
            InitializeComponent();
        }
    }
}
