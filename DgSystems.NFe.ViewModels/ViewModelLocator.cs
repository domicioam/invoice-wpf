/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:EmissorNFe"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using DgSystems.NFe.ViewModels;
using NFe.WPF.NotaFiscal.ViewModel;
using NFe.WPF.ViewModel;

using SimpleInjector;

namespace EmissorNFe.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        private Container _container;

        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator(Container container)
        {
            //_container = DependencyResolver.Container;
            _container = container;
        }

        public MainViewModel Main
        {
            get
            {
                return _container.GetInstance<MainViewModel>();
            }
        }

        public EnviarContabilidadeViewModel EnviarContabilidade
        {
            get
            {
                return _container.GetInstance<EnviarContabilidadeViewModel>();
            }
        }

        public EnviarEmailViewModel EnviarEmail
        {
            get
            {
                return _container.GetInstance<EnviarEmailViewModel>();
            }
        }

        public AcompanhamentoNotasViewModel Acompanhamento
        {
            get
            {
                return _container.GetInstance<AcompanhamentoNotasViewModel>();
            }
        }

        public VisualizarNotaEnviadaViewModel VisualizarNotaEnviada
        {
            get
            {
                return _container.GetInstance<VisualizarNotaEnviadaViewModel>();
            }
        }

        public CancelarNotaViewModel CancelarNota
        {
            get
            {
                return _container.GetInstance<CancelarNotaViewModel>();
            }
        }

        public DestinatarioViewModel Destinatario
        {
            get
            {
                return _container.GetInstance<DestinatarioViewModel>();
            }
        }

        public NFeViewModel NFe
        {
            get
            {
                return _container.GetInstance<NFeViewModel>();
            }
        }

        public ImpostoViewModel Imposto
        {
            get
            {
                return _container.GetInstance<ImpostoViewModel>();
            }
        }

        public ProdutoViewModel Produto
        {
            get
            {
                return _container.GetInstance<ProdutoViewModel>();
            }
        }

        public NFCeViewModel NFCe
        {
            get
            {
                return _container.GetInstance<NFCeViewModel>();
            }
        }

        public EmitenteViewModel Empresa
        {
            get
            {
                return _container.GetInstance<EmitenteViewModel>();
            }
        }

        public OpcoesViewModel Opcoes
        {
            get
            {
                return _container.GetInstance<OpcoesViewModel>();
            }
        }

        public CertificadoViewModel Certificado
        {
            get
            {
                return _container.GetInstance<CertificadoViewModel>();
            }
        }

        public NotaFiscalMainViewModel NotaFiscalMain
        {
            get
            {
                return _container.GetInstance<NotaFiscalMainViewModel>();
            }
        }

        public DestinatarioMainViewModel DestinatarioMain
        {
            get
            {
                return _container.GetInstance<DestinatarioMainViewModel>();
            }
        }

        public ProdutoMainViewModel ProdutoMain
        {
            get
            {
                return _container.GetInstance<ProdutoMainViewModel>();
            }
        }

        public ImpostoMainViewModel ImpostoMain
        {
            get
            {
                return _container.GetInstance<ImpostoMainViewModel>();
            }
        }

        public ImportarXMLViewModel ImportarXML
        {
            get
            {
                return _container.GetInstance<ImportarXMLViewModel>();
            }
        }

        public ImportarXMLFornecedorViewModel ImportarXMLFornecedor
        {
            get
            {
                return _container.GetInstance<ImportarXMLFornecedorViewModel>();
            }
        }

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}