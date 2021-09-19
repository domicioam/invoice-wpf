using Akka.Actor;
using Akka.TestKit.Xunit2;
using Akka.Util;
using DgSystems.NFe.Services.Actors;
using Moq;
using NFe.Core.Cadastro.Configuracoes;
using NFe.Core.Domain;
using NFe.Core.Interfaces;
using NFe.Core.NFeAutorizacao4;
using NFe.Core.NotasFiscais;
using NFe.Core.NotasFiscais.Sefaz.NfeConsulta2;
using NFe.Core.Sefaz.Facades;
using NFe.Core.XmlSchemas.NfeAutorizacao.Retorno;
using System;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Xml;
using Xunit;

namespace DgSystems.NFe.Core.UnitTests.Services.Actors
{
    public class EnviarNotaActorTest : TestKit, IClassFixture<NotaFiscalFixture>
    {
        private readonly NotaFiscalFixture fixture;

        public EnviarNotaActorTest(NotaFiscalFixture fixture)
        {
            this.fixture = fixture;
            AppContext.SetSwitch("Switch.System.Security.Cryptography.Xml.UseInsecureHashAlgorithms", true);
            AppContext.SetSwitch("Switch.System.Security.Cryptography.Pkcs.UseInsecureHashAlgorithms", true);
        }

        [Fact]
        public void Should_send_nota_to_sefaz_when_nota_is_valid()
        {
            var configuracaoRepositoryMock = new Mock<IConfiguracaoRepository>();
            var serviceFactoryMock = new Mock<IServiceFactory>();
            var nfeConsultaMock = new Mock<IConsultarNotaFiscalService>();
            var emiteNotaContingenciaServiceMock = new Mock<IEmiteNotaFiscalContingenciaFacade>();
            var nfeAutorizacaoSoapMock = new Mock<NFeAutorizacao4Soap>();

            nfeAutorizacaoSoapMock.Setup(n => n.nfeAutorizacaoLote(It.IsAny<nfeAutorizacaoLoteRequest>()))
                .Returns(new nfeAutorizacaoLoteResponse() { nfeResultMsg = fixture.nfeResultMsg });

            configuracaoRepositoryMock.Setup(c => c.GetConfiguracaoAsync()).Returns(Task.FromResult(new ConfiguracaoEntity()));
            serviceFactoryMock.Setup(s =>
                s.GetService(It.IsAny<Modelo>(), It.IsAny<Servico>(), It.IsAny<CodigoUfIbge>(), It.IsAny<X509Certificate2>()))
                .Returns(new Service() { SoapClient = nfeAutorizacaoSoapMock.Object });

            XmlNFe xmlNFe = new XmlNFe(fixture.NotaFiscal, fixture.NfeNamespaceName, fixture.X509Certificate2, fixture.CscId, fixture.Csc);
            var message = new EnviarNotaActor.EnviarNotaFiscal(fixture.NotaFiscal, fixture.CscId, fixture.Csc, fixture.X509Certificate2, xmlNFe);
            
            var subject = Sys.ActorOf(Props.Create(() => new EnviarNotaActor(
                configuracaoRepositoryMock.Object, serviceFactoryMock.Object, nfeConsultaMock.Object,
                emiteNotaContingenciaServiceMock.Object)));
            subject.Tell(message);
            ExpectMsg<Status.Success>(TimeSpan.FromSeconds(10));
        }

        [Fact]
        public void Should_emitir_em_contingencia_quando_contingencia_ativada()
        {
            // Given
            var configuracaoRepositoryMock = new Mock<IConfiguracaoRepository>();
            var serviceFactoryMock = new Mock<IServiceFactory>();
            var nfeConsultaMock = new Mock<IConsultarNotaFiscalService>();
            var emiteNotaContingenciaServiceMock = new Mock<IEmiteNotaFiscalContingenciaFacade>();
            var nfeAutorizacaoSoapMock = new Mock<NFeAutorizacao4Soap>();

            configuracaoRepositoryMock.Setup(c => c.GetConfiguracaoAsync()).Returns(Task.FromResult(new ConfiguracaoEntity() { IsContingencia = true}));
            serviceFactoryMock.Setup(s => s.GetService(It.IsAny<Modelo>(), It.IsAny<Servico>(), It.IsAny<CodigoUfIbge>(), It.IsAny<X509Certificate2>()))
                .Returns(new Service() { SoapClient = nfeAutorizacaoSoapMock.Object });

            XmlNFe xmlNFe = new XmlNFe(fixture.NotaFiscal, fixture.NfeNamespaceName, fixture.X509Certificate2, fixture.CscId, fixture.Csc);
            var message = new EnviarNotaActor.EnviarNotaFiscal(fixture.NotaFiscal, fixture.CscId, fixture.Csc, fixture.X509Certificate2, xmlNFe);
            var subject = Sys.ActorOf(Props.Create(() => new EnviarNotaActor(configuracaoRepositoryMock.Object, serviceFactoryMock.Object, nfeConsultaMock.Object, emiteNotaContingenciaServiceMock.Object)));

            // When
            subject.Tell(message);

            // Then
            ExpectMsg<Status.Success>(TimeSpan.FromSeconds(10));
            emiteNotaContingenciaServiceMock.Verify(e => e.SaveNotaFiscalContingenciaAsync(It.IsAny<X509Certificate2>(),
                It.IsAny<ConfiguracaoEntity>(), It.IsAny<NotaFiscal>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));
        }

        [Fact]
        public void Should_retornar_sucesso_apos_resposta_sefaz_actor_success()
        {
            // Given
            var configuracaoRepositoryMock = new Mock<IConfiguracaoRepository>();
            var serviceFactoryMock = new Mock<IServiceFactory>();
            var nfeConsultaMock = new Mock<IConsultarNotaFiscalService>();
            var emiteNotaContingenciaServiceMock = new Mock<IEmiteNotaFiscalContingenciaFacade>();
            var nfeAutorizacaoSoapMock = new Mock<NFeAutorizacao4Soap>();

            nfeAutorizacaoSoapMock.Setup(n => n.nfeAutorizacaoLote(It.IsAny<nfeAutorizacaoLoteRequest>()))
                .Returns(new nfeAutorizacaoLoteResponse() { nfeResultMsg = fixture.nfeResultMsg });

            configuracaoRepositoryMock.Setup(c => c.GetConfiguracaoAsync()).Returns(Task.FromResult(new ConfiguracaoEntity()));
            serviceFactoryMock.Setup(s => s.GetService(It.IsAny<Modelo>(), It.IsAny<Servico>(), It.IsAny<CodigoUfIbge>(), It.IsAny<X509Certificate2>()))
                .Returns(new Service() { SoapClient = nfeAutorizacaoSoapMock.Object });

            XmlNFe xmlNFe = new XmlNFe(fixture.NotaFiscal, fixture.NfeNamespaceName, fixture.X509Certificate2, fixture.CscId, fixture.Csc);
            var request = new EnviarNotaActor.EnviarNotaFiscal(fixture.NotaFiscal, fixture.CscId, fixture.Csc, fixture.X509Certificate2, xmlNFe);
            var response = new Result<TProtNFe>(fixture.ProtNFe);
            var subject = Sys.ActorOf(Props.Create(() => new EnviarNotaActor(configuracaoRepositoryMock.Object, serviceFactoryMock.Object, nfeConsultaMock.Object, emiteNotaContingenciaServiceMock.Object)));

            // When
            subject.Tell(request);
            subject.Tell(response);

            // Then
            var resultadoEnvio = ExpectMsg<Status.Success>(TimeSpan.FromSeconds(10)).Status as ResultadoEnvio;
            Assert.NotNull(resultadoEnvio);
            Assert.Equal(fixture.ProtNFe.infProt.nProt, resultadoEnvio.NotaFiscal.ProtocoloAutorizacao);
            Assert.Equal(DateTime.ParseExact(fixture.ProtNFe.infProt.dhRecbto, NotaFiscalFixture.DATE_STRING_SEFAZ_FORMAT, CultureInfo.InvariantCulture), resultadoEnvio.NotaFiscal.DataHoraAutorização);
            Assert.Equal(global::NFe.Core.Entitities.Status.ENVIADA, resultadoEnvio.NotaFiscal.Identificacao.Status.Status);
        }
    }
}
