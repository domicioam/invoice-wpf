using DgSystems.NFe.ViewModels.Commands;
using EmissorNFe.Certificado;
using EmissorNFe.Imposto;
using EmissorNFe.NotaFiscal;
using EmissorNFe.Produto;
using EmissorNFe.View;
using EmissorNFe.View.Configurações;
using EmissorNFe.View.Destinatario;
using EmissorNFe.View.Destinatário;
using EmissorNFe.View.Emitente;
using EmissorNFe.View.Gerencial;
using EmissorNFe.View.NotaFiscal;
using NFe.Core.Messaging;
using NFe.WPF.Acompanhamento.View;
using NFe.WPF.Commands;
using NFe.WPF.View.Ferramentas;
using NFe.WPF.View.NotaFiscal;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EmissorNFe
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            ContentHolder.Content = new NotaFiscalMainWindow();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

            MessagingCenter.Subscribe<DestinatarioMainViewModel, AlterarDestinatarioCommand>(this, nameof(AlterarDestinatarioCommand), (s, e) =>
            {
                var app = Application.Current;
                var mainWindow = app.MainWindow;
                new DestinatarioWindow(e.DestinatarioViewModel) { Owner = mainWindow }.ShowDialog();
            });

            MessagingCenter.Subscribe<CancelarNotaViewModel, CancelarNotaFiscalCommand>(this, nameof(CancelarNotaFiscalCommand), (s, e) =>
            {
                var app = Application.Current;
                var mainWindow = app.MainWindow;
                new CancelarNotaWindow(e.CancelarNotaViewModel) { Owner = mainWindow }.ShowDialog();
            });

            MessagingCenter.Subscribe<EnviarEmailViewModel, OpenEnviarEmailWindowCommand>(this, nameof(OpenEnviarEmailWindowCommand), (s, e) =>
            {
                var app = Application.Current;
                var mainWindow = app.MainWindow;
                var window = new EnviarEmailWindow(e.EnviarEmailViewModel) { Owner = mainWindow };
                window.ShowDialog();
            });

            MessagingCenter.Subscribe<ImpostoViewModel, OpenCadastroImpostoWindowCommand>(this, nameof(OpenCadastroImpostoWindowCommand), (s, e) =>
            {
                var app = Application.Current;
                var mainWindow = app.MainWindow;

                CadastroImpostoWindow cadastroImpostoWindow = new CadastroImpostoWindow() { Owner = mainWindow };
                cadastroImpostoWindow.DataContext = e.ImpostoViewModel;
                cadastroImpostoWindow.ShowDialog();
            });

            MessagingCenter.Subscribe<VisualizarNotaEnviadaViewModel, OpenVisualizarNotaEnviadaWindowCommand>(this, nameof(OpenVisualizarNotaEnviadaWindowCommand), (s, e) =>
            {
                var app = Application.Current;
                var mainWindow = app.MainWindow;

                new VisualizarNotaEnviadaWindow(e.VisualizarNotaEnviadaViewModel) { Owner = mainWindow }.ShowDialog();
            });

            MessagingCenter.Subscribe<ProdutoViewModel, OpenCadastroProdutoWindowCommand>(this, nameof(OpenCadastroProdutoWindowCommand), (s, e) =>
            {
                var app = Application.Current;
                var mainWindow = app.MainWindow;

                CadastroProdutoWindow cadastroProdutoWindow = new CadastroProdutoWindow() { Owner = mainWindow };
                cadastroProdutoWindow.DataContext = e.ProdutoViewModel;
                cadastroProdutoWindow.ShowDialog();
            });
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            new AdicionarCertificadoWindow() { Owner = this }.ShowDialog();
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            ContentHolder.Content = new ProdutoMainWindow();
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            ContentHolder.Content = new NotaFiscalMainWindow();
        }

        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            ContentHolder.Content = new ImpostoMainWindow();
        }

        private void MenuItem_Click_4(object sender, RoutedEventArgs e)
        {
            new CadastrarEmpresaWindow() { Owner = this }.ShowDialog();
        }

        private void MenuItem_Click_5(object sender, RoutedEventArgs e)
        {
            new OpcoesWindows() { Owner = this }.ShowDialog();
        }

        private void MenuItem_Click_6(object sender, RoutedEventArgs e)
        {
            ContentHolder.Content = new DestinatarioMainWindow();
        }

        private void MenuItem_Click_7(object sender, RoutedEventArgs e)
        {
            new EnvioRelatorioContabilidadeWindow() { Owner = this }.ShowDialog();
        }

        private void MenuItem_Click_8(object sender, RoutedEventArgs e)
        {
            new AcompanhamentoNotas() { Owner = this }.Show();
        }

        private void MenuItem_Click_9(object sender, RoutedEventArgs e)
        {
            new ImportarXMLWindow() { Owner = this }.Show();
        }

        private void MenuItem_Click_10(object sender, RoutedEventArgs e)
        {
            new ImportarXMLFornecedorWindow() { Owner = this }.Show();
        }
    }
}
