using EmissorNFe.ViewModel;
using GalaSoft.MvvmLight.Views;
using NFe.Core.Cadastro.Certificado;
using NFe.Core.Cadastro.Configuracoes;
using NFe.Core.Cadastro.Destinatario;
using NFe.Core.Cadastro.Emissor;
using NFe.Core.Cadastro.Transportadora;
using NFe.Core.Interfaces;
using NFe.Core.NotasFiscais;
using NFe.Core.NotasFiscais.Sefaz.NfeConsulta2;
using NFe.Core.NotasFiscais.Sefaz.NfeInutilizacao2;
using NFe.Core.NotasFiscais.Sefaz.NfeRecepcaoEvento;
using NFe.Core.NotasFiscais.Services;
using NFe.Core.Sefaz;
using NFe.Core.Sefaz.Facades;
using NFe.Core.Utils;
using NFe.Core.Utils.Assinatura;
using NFe.Core.Utils.PDF;
using NFe.Core.Utils.Zip;
using NFe.Repository.Repositories;
using NFe.WPF.NotaFiscal.ViewModel;
using NFe.WPF.Utils;
using NFe.WPF.View;
using NFe.WPF.ViewModel;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DgSystem.NFe.IoC
{
    public class DependencyResolver
    {
        public static void RegisterTypes()
        {
            var container = new Container();

            container.Register<MainViewModel>(Lifestyle.Singleton);
            container.Register<ImpostoViewModel>(Lifestyle.Singleton);
            container.Register<ProdutoViewModel>(Lifestyle.Singleton);
            container.Register<EmitenteViewModel>(Lifestyle.Singleton);
            container.Register<OpcoesViewModel>(Lifestyle.Singleton);
            container.Register<CertificadoViewModel>(Lifestyle.Singleton);
            container.Register<NFCeViewModel>(Lifestyle.Singleton);
            container.Register<NotaFiscalMainViewModel>(Lifestyle.Singleton);
            container.Register<NFeViewModel>(Lifestyle.Singleton);
            container.Register<DestinatarioMainViewModel>(Lifestyle.Singleton);
            container.Register<ProdutoMainViewModel>(Lifestyle.Singleton);
            container.Register<ImpostoMainViewModel>(Lifestyle.Singleton);
            container.Register<DestinatarioViewModel>(Lifestyle.Singleton);
            container.Register<CancelarNotaViewModel>(Lifestyle.Singleton);
            container.Register<VisualizarNotaEnviadaViewModel>(Lifestyle.Singleton);
            container.Register<EnviarContabilidadeViewModel>(Lifestyle.Singleton);
            container.Register<AcompanhamentoNotasViewModel>(Lifestyle.Singleton);
            container.Register<EnviarEmailViewModel>(Lifestyle.Singleton);
            container.Register<ImportarXMLViewModel>(Lifestyle.Singleton);

            container.Register<IDialogService, MessageService>(Lifestyle.Singleton);

            container.Register<INFeConsulta, NFeConsulta>(Lifestyle.Singleton);
            container.Register<RijndaelManagedEncryption>(Lifestyle.Singleton);
            container.Register<ICertificateManager, CertificateManager>(Lifestyle.Singleton);
            container.Register<IEnviarNota, EnviarNotaController>(Lifestyle.Singleton);
            container.Register<IEnviaNotaFiscalFacade, EnviaNotaFiscalFacade>(Lifestyle.Singleton);
            container.Register<IServiceFactory, ServiceFactory>(Lifestyle.Singleton);
            container.Register<INotaInutilizadaService, NotaInutilizadaService>(Lifestyle.Singleton);
            container.Register<IEventoService, EventoService>(Lifestyle.Singleton);
            container.Register<ImportadorXmlService>(Lifestyle.Singleton);
            container.Register<IEmissorService, EmissorService>(Lifestyle.Singleton);
            container.Register<ICertificadoService, CertificadoService>(Lifestyle.Singleton);
            container.Register<IConfiguracaoService, ConfiguracaoService>(Lifestyle.Singleton);
            container.Register<IConsultaStatusServicoFacade, ConsultaStatusServicoFacade>(Lifestyle.Singleton);
            container.Register<IEventoRepository, EventoRepository>(Lifestyle.Singleton);
            container.Register<INotaFiscalRepository, NotaFiscalRepository>(Lifestyle.Singleton);
            container.Register<ICertificadoRepository, CertificadoRepository>(Lifestyle.Singleton);
            container.Register<INotaInutilizadaRepository, NotaInutilizadaRepository>(Lifestyle.Singleton);
            container.Register<IConfiguracaoRepository, ConfiguracaoRepository>(Lifestyle.Singleton);
            container.Register<IDestinatarioRepository, DestinatarioRepository>(Lifestyle.Singleton);
            container.Register<IEmitenteRepository, EmitenteRepository>(Lifestyle.Singleton);
            container.Register<IGrupoImpostosRepository, GrupoImpostosRepository>(Lifestyle.Singleton);
            container.Register<IProdutoRepository, ProdutoRepository>(Lifestyle.Singleton);
            container.Register<ITransportadoraRepository, TransportadoraRepository>(Lifestyle.Singleton);
            container.Register<IEstadoRepository, EstadoRepository>(Lifestyle.Singleton);
            container.Register<IMunicipioRepository, MunicipioRepository>(Lifestyle.Singleton);
            container.Register<INaturezaOperacaoRepository, NaturezaOperacaoRepository>(Lifestyle.Singleton);
            container.Register<IHistoricoEnvioContabilidadeRepository, HistoricoEnvioContabilidadeRepository>(Lifestyle.Singleton);
            container.Register<IEmiteNotaFiscalContingenciaFacade, EmiteEmiteNotaFiscalContingenciaFacade>(Lifestyle.Singleton);
            container.Register<ICancelaNotaFiscalFacade, CancelaNotaFiscalFacade>(Lifestyle.Singleton);
            container.Register<NFeInutilizacao>(Lifestyle.Singleton);
            container.Register<INFeCancelamento, NFeCancelamento>(Lifestyle.Singleton);
            container.Register<MailManager>(Lifestyle.Singleton);
            container.Register<ModoOnlineService>(Lifestyle.Singleton);
            container.Register<IDestinatarioService, DestinatarioService>(Lifestyle.Singleton);
            container.Register<GeradorZip>(Lifestyle.Singleton);
            container.Register<GeradorPDF>(Lifestyle.Singleton);
            container.Register<ITransportadoraService, TransportadoraService>(Lifestyle.Singleton);
            container.Register<SefazSettings>(Lifestyle.Singleton);
            container.Register<InutilizarNotaFiscalFacade>(Lifestyle.Singleton);

            container.Verify();

            Container = container;
        }

        public static Container Container { get; set; }
    }
}
