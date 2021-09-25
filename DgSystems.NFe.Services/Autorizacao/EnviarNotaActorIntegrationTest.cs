using Akka.Actor;
using Akka.TestKit.Xunit2;
using AutoMapper;
using DgSystems.NFe.Core.UnitTests;
using DgSystems.NFe.Services.Actors;
using Moq;
using NFe.Core.Cadastro.Certificado;
using NFe.Core.Cadastro.Configuracoes;
using NFe.Core.Domain;
using NFe.Core.Entitities;
using NFe.Core.Events;
using NFe.Core.Interfaces;
using NFe.Core.Messaging;
using NFe.Core.NFeAutorizacao4;
using NFe.Core.NotasFiscais;
using NFe.Core.NotasFiscais.Sefaz.NfeConsulta2;
using NFe.Core.NotasFiscais.Services;
using NFe.Core.Sefaz;
using NFe.Core.Sefaz.Facades;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Xunit;

namespace DgSystems.NFe.Services.UnitTests.Autorizacao
{
    public class EnviarNotaActorIntegrationTest : TestKit, IClassFixture<NotaFiscalFixture>
    {
        private readonly NotaFiscalFixture fixture;
        Mock<IConfiguracaoRepository> configuracaoRepository = new Mock<IConfiguracaoRepository>();
        Mock<IConsultaStatusServicoSefazService> consultaStatusServicoService = new Mock<IConsultaStatusServicoSefazService>();
        Mock<INotaFiscalRepository> notaFiscalRepository = new Mock<INotaFiscalRepository>();
        Mock<IEmiteNotaFiscalContingenciaFacade> emiteNotaFiscalContingenciaService = new Mock<IEmiteNotaFiscalContingenciaFacade>();
        Mock<IEmitenteRepository> emissorService = new Mock<IEmitenteRepository>();
        Mock<IConsultarNotaFiscalService> nfeConsulta = new Mock<IConsultarNotaFiscalService>();
        Mock<IServiceFactory> serviceFactory = new Mock<IServiceFactory>();
        Mock<CertificadoService> certificadoService = new Mock<CertificadoService>();
        Mock<SefazSettings> sefazSettings = new Mock<SefazSettings>();
        Mock<IServiceFactory> serviceFactoryMock = new Mock<IServiceFactory>();
        Mock<NFeAutorizacao4Soap> nfeAutorizacaoSoapMock = new Mock<NFeAutorizacao4Soap>();
        Mock<IMapper> mapper = new Mock<IMapper>();

        public EnviarNotaActorIntegrationTest(NotaFiscalFixture fixture)
        {
            this.fixture = fixture;
            AppContext.SetSwitch("Switch.System.Security.Cryptography.Xml.UseInsecureHashAlgorithms", true);
            AppContext.SetSwitch("Switch.System.Security.Cryptography.Pkcs.UseInsecureHashAlgorithms", true);
        }

        [Fact]
        public void Deve_enviar_nota_em_contingencia_quando_offline()
        {
            // ativa modo offline
            configuracaoRepository.Setup(c => c.GetConfiguracao()).Returns(new ConfiguracaoEntity() { IsContingencia = false, JustificativaContingencia = null });
            consultaStatusServicoService.Setup(c => c.ExecutarConsultaStatus(It.IsAny<Modelo>(), ConfigurationManager.AppSettings["sefazEnvironment"])).Returns(false);
            var contingenciaActor = CreateTestProbe();

            var modoOnlineActor = Sys.ActorOf(Props.Create(() => new ModoOnlineActor(
                configuracaoRepository.Object, consultaStatusServicoService.Object, notaFiscalRepository.Object,
                emiteNotaFiscalContingenciaService.Object, emissorService.Object, nfeConsulta.Object,
                serviceFactory.Object, certificadoService.Object, sefazSettings.Object, _ => contingenciaActor.Ref)));

            modoOnlineActor.Tell(new ModoOnlineActor.Start());

            contingenciaActor.ExpectNoMsg();
            configuracaoRepository.Verify(c => c.Salvar(It.Is<ConfiguracaoEntity>(config => config.IsContingencia && !string.IsNullOrEmpty(config.JustificativaContingencia))), Times.Once);
            Task.Delay(TimeSpan.FromSeconds(1)).Wait();

            // envia nota para enviar nota actor

            nfeAutorizacaoSoapMock.Setup(n => n.nfeAutorizacaoLote(It.IsAny<nfeAutorizacaoLoteRequest>()))
                .Returns(new nfeAutorizacaoLoteResponse() { nfeResultMsg = fixture.nfeResultMsg });

            configuracaoRepository.Setup(c => c.GetConfiguracaoAsync()).Returns(Task.FromResult(new ConfiguracaoEntity() { IsContingencia = true, JustificativaContingencia = "justificativa" }));
            serviceFactoryMock.Setup(s =>
                s.GetService(It.IsAny<Modelo>(), It.IsAny<Servico>(), It.IsAny<CodigoUfIbge>(), It.IsAny<X509Certificate2>()))
                .Returns(new Service() { SoapClient = nfeAutorizacaoSoapMock.Object });

            XmlNFe xmlNFe = new XmlNFe(fixture.NotaFiscal, fixture.NfeNamespaceName, fixture.X509Certificate2, fixture.CscId, fixture.Csc);
            var message = new EnviarNotaActor.EnviarNotaFiscal(fixture.NotaFiscal, fixture.CscId, fixture.Csc, fixture.X509Certificate2, xmlNFe);

            var notaFiscalActor = Sys.ActorOf(Props.Create(() => new EnviarNotaActor(
                configuracaoRepository.Object, serviceFactoryMock.Object, nfeConsulta.Object,
                emiteNotaFiscalContingenciaService.Object, mapper.Object, _ => contingenciaActor.Ref, modoOnlineActor)));
            notaFiscalActor.Tell(message);
            ExpectNoMsg();
            nfeAutorizacaoSoapMock.Verify(n => n.nfeAutorizacaoLote(It.IsAny<nfeAutorizacaoLoteRequest>()), Times.Never);

            // mensagem deve ser encaminhada para nota fiscal contingencia actor

            var saveContingencia = contingenciaActor.ExpectMsg<EmiteNFeContingenciaActor.SaveNotaFiscalContingencia>();
            Assert.Equal(fixture.NotaFiscal, saveContingencia.notaFiscal);
        }
    }
}
