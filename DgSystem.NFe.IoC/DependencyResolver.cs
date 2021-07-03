using Akka.Actor;
using DgSystems.NFe.NotaFiscal.Reports;
using DgSystems.NFe.ViewModels;
using EmissorNFe.ViewModel;
using GalaSoft.MvvmLight.Views;
using MediatR;
using MediatR.SimpleInjector;
using NFe.Core;
using NFe.Core.Cadastro.Certificado;
using NFe.Core.Cadastro.Configuracoes;
using NFe.Core.Cadastro.Destinatario;
using NFe.Core.Cadastro.Emissor;
using NFe.Core.Cadastro.Ibpt;
using NFe.Core.Cadastro.Imposto;
using NFe.Core.Interfaces;
using NFe.Core.NotasFiscais;
using NFe.Core.NotasFiscais.Repositories;
using NFe.Core.NotasFiscais.Sefaz.NfeConsulta2;
using NFe.Core.NotasFiscais.Services;
using NFe.Core.Sefaz;
using NFe.Core.Sefaz.Facades;
using NFe.Core.Utils;
using NFe.Core.Utils.PDF;
using NFe.Core.Utils.Zip;
using NFe.Repository.Repositories;
using NFe.WPF.NotaFiscal.ViewModel;
using NFe.WPF.Utils;
using NFe.WPF.View;
using NFe.WPF.ViewModel;
using SimpleInjector;
using System.Collections.Generic;
using System.Reflection;
using ServiceFactory = NFe.Core.NotasFiscais.ServiceFactory;

namespace DgSystem.NFe.IoC
{
    public static class DependencyResolver
    {
        public static void RegisterTypes()
        {
            var container = new Container();

            container.Register<MainViewModel>(Lifestyle.Transient);
            container.Register<ImpostoViewModel>(Lifestyle.Transient);
            container.Register<ProdutoViewModel>(Lifestyle.Transient);
            container.Register<EmitenteViewModel>(Lifestyle.Transient);
            container.Register<OpcoesViewModel>(Lifestyle.Transient);
            container.Register<CertificadoViewModel>(Lifestyle.Transient);
            container.Register<NFCeViewModel>(Lifestyle.Transient);
            container.Register<NotaFiscalMainViewModel>(Lifestyle.Transient);
            container.Register<NFeViewModel>(Lifestyle.Transient);
            container.Register<DestinatarioMainViewModel>(Lifestyle.Transient);
            container.Register<ProdutoMainViewModel>(Lifestyle.Transient);
            container.Register<ImpostoMainViewModel>(Lifestyle.Transient);
            container.Register<DestinatarioViewModel>(Lifestyle.Transient);
            container.Register<CancelarNotaViewModel>(Lifestyle.Transient);
            container.Register<VisualizarNotaEnviadaViewModel>(Lifestyle.Transient);
            container.Register<EnviarContabilidadeViewModel>(Lifestyle.Transient);
            container.Register<AcompanhamentoNotasViewModel>(Lifestyle.Transient);
            container.Register<EnviarEmailViewModel>(Lifestyle.Transient);
            container.Register<ImportarXMLViewModel>(Lifestyle.Transient);

            container.Register<IDialogService, MessageService>(Lifestyle.Transient);

            container.Register<IConsultarNotaFiscalService, ConsultarNotaFiscalService>(Lifestyle.Transient);
            container.Register<RijndaelManagedEncryption>(Lifestyle.Transient);
            container.Register<IEnviarNotaAppService, EnviarNotaAppService>(Lifestyle.Transient);
            container.Register<IServiceFactory, ServiceFactory>(Lifestyle.Transient);
            container.Register<ImportadorXmlService>(Lifestyle.Transient);
            container.Register<ICertificadoService, CertificadoService>(Lifestyle.Transient);
            container.Register<IConsultaStatusServicoSefazService, ConsultaStatusServicoSefazService>(Lifestyle.Transient);
            container.Register<IEventoRepository, EventoRepository>(Lifestyle.Transient);
            container.Register<INotaFiscalRepository, NotaFiscalRepository>(Lifestyle.Transient);
            container.Register<ICertificadoRepository, CertificadoRepository>(Lifestyle.Transient);
            container.Register<INotaInutilizadaRepository, NotaInutilizadaRepository>(Lifestyle.Transient);
            container.Register<IConfiguracaoRepository, ConfiguracaoRepository>(Lifestyle.Transient);
            container.Register<IDestinatarioRepository, DestinatarioRepository>(Lifestyle.Transient);
            container.Register<IEmitenteRepository, EmitenteRepository>(Lifestyle.Transient);
            container.Register<IGrupoImpostosRepository, GrupoImpostosRepository>(Lifestyle.Transient);
            container.Register<IProdutoRepository, ProdutoRepository>(Lifestyle.Transient);
            container.Register<ITransportadoraRepository, TransportadoraRepository>(Lifestyle.Transient);
            container.Register<IEstadoRepository, EstadoRepository>(Lifestyle.Transient);
            container.Register<IMunicipioRepository, MunicipioRepository>(Lifestyle.Transient);
            container.Register<INaturezaOperacaoRepository, NaturezaOperacaoRepository>(Lifestyle.Transient);
            container.Register<IHistoricoEnvioContabilidadeRepository, HistoricoEnvioContabilidadeRepository>(Lifestyle.Transient);
            container.Register<IEmiteNotaFiscalContingenciaFacade, EmiteNotaFiscalContingenciaFacade>(Lifestyle.Transient);
            container.Register<ICancelaNotaFiscalService, CancelaNotaFiscalService>(Lifestyle.Transient);
            container.Register<MailManager>(Lifestyle.Transient);
            container.Register<GeradorZip>(Lifestyle.Transient);
            container.Register<GeradorPDF>(Lifestyle.Transient);
            container.Register<SefazSettings>(Lifestyle.Transient);
            container.Register<InutilizarNotaFiscalService>(Lifestyle.Transient);
            container.Register<XmlUtil>(Lifestyle.Transient);
            container.Register<IIbptManager, IbptManager>(Lifestyle.Transient);

            var actorSystem = ActorSystem.Create("ActorSystem");

            container.RegisterInstance<ActorSystem>(actorSystem);


            container.BuildMediator(GetAssemblies());

            container.Verify();

            Container = container;
        }

        public static Container Container { get; set; }

        private static IEnumerable<Assembly> GetAssemblies()
        {
            yield return typeof(ImprimirDanfe).GetTypeInfo().Assembly;
            yield return typeof(ImprimirDanfeHandler).GetTypeInfo().Assembly;
        }
    }
}
