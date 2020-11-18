using DgSystems.NFe.ViewModels.Commands;
using EmissorNFe.Imposto;
using EmissorNFe.Model;
using EmissorNFe.Produto;
using EmissorNFe.View.Destinatario;
using EmissorNFe.View.NotaFiscal;
using EmissorNFe.ViewModel;
using NFe.Core.Messaging;
using NFe.WPF.Commands;
using NFe.WPF.View.NotaFiscal;
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

namespace EmissorNFe.NotaFiscal
{
    /// <summary>
    /// Interaction logic for NotaFiscalMainWindow.xaml
    /// </summary>
    public partial class NotaFiscalMainWindow : UserControl
    {
        public NotaFiscalMainWindow()
        {
            InitializeComponent();

            MessagingCenter.Subscribe<NotaFiscalMainWindow, AlterarDestinatarioCommand>(this, nameof(AlterarDestinatarioCommand), (s, e) =>
            {
                var app = Application.Current;
                var mainWindow = app.MainWindow;
                new DestinatarioWindow(e.DestinatarioViewModel) { Owner = mainWindow }.ShowDialog();
            });

            MessagingCenter.Subscribe<NotaFiscalMainWindow, CancelarNotaFiscalCommand>(this, nameof(CancelarNotaFiscalCommand), (s, e) =>
            {
                var app = Application.Current;
                var mainWindow = app.MainWindow;
                new CancelarNotaWindow(e.CancelarNotaViewModel) { Owner = mainWindow }.ShowDialog();
            });

            MessagingCenter.Subscribe<NotaFiscalMainWindow, CancelarNotaFiscalCommand>(this, nameof(CancelarNotaFiscalCommand), (s, e) =>
            {
                var app = Application.Current;
                var mainWindow = app.MainWindow;
                new CancelarNotaWindow(e.CancelarNotaViewModel) { Owner = mainWindow }.ShowDialog();
            });


            MessagingCenter.Subscribe<NotaFiscalMainWindow, OpenEnviarEmailWindowCommand>(this, nameof(OpenEnviarEmailWindowCommand), (s, e) =>
            {
                var app = Application.Current;
                var mainWindow = app.MainWindow;
                var window = new EnviarEmailWindow(e.EnviarEmailViewModel) { Owner = mainWindow };
                window.ShowDialog();
            });

            MessagingCenter.Subscribe<NotaFiscalMainWindow, OpenCadastroImpostoWindowCommand>(this, nameof(OpenCadastroImpostoWindowCommand), (s, e) =>
            {
                var app = Application.Current;
                var mainWindow = app.MainWindow;

                CadastroImpostoWindow cadastroImpostoWindow = new CadastroImpostoWindow() { Owner = mainWindow };
                cadastroImpostoWindow.DataContext = e.ImpostoViewModel;
                cadastroImpostoWindow.ShowDialog();
            });

            MessagingCenter.Subscribe<NotaFiscalMainWindow, OpenVisualizarNotaEnviadaWindowCommand>(this, nameof(OpenVisualizarNotaEnviadaWindowCommand), (s, e) =>
            {
                var app = Application.Current;
                var mainWindow = app.MainWindow;

                new VisualizarNotaEnviadaWindow(e.VisualizarNotaEnviadaViewModel) { Owner = mainWindow }.ShowDialog();
            });

            MessagingCenter.Subscribe<NotaFiscalMainWindow, OpenCadastroProdutoWindowCommand>(this, nameof(OpenCadastroProdutoWindowCommand), (s, e) =>
            {
                var app = Application.Current;
                var mainWindow = app.MainWindow;

                CadastroProdutoWindow cadastroProdutoWindow = new CadastroProdutoWindow() { Owner = mainWindow };
                cadastroProdutoWindow.DataContext = e.ProdutoViewModel;
                cadastroProdutoWindow.ShowDialog();
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var app = Application.Current;
            var mainWindow = app.MainWindow;

            new NFCeWindow { Owner = mainWindow }.ShowDialog();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var app = Application.Current;
            var mainWindow = app.MainWindow;

            new NFeWindow { Owner = mainWindow }.ShowDialog();
        }
    }
}
