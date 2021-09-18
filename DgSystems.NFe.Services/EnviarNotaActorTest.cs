using Akka.Actor;
using Akka.TestKit.Xunit2;
using DgSystems.NFe.Services.Actors;
using Moq;
using NFe.Core.Cadastro.Configuracoes;
using NFe.Core.Domain;
using NFe.Core.Interfaces;
using NFe.Core.NFeAutorizacao4;
using NFe.Core.NotasFiscais;
using NFe.Core.NotasFiscais.Sefaz.NfeConsulta2;
using NFe.Core.Sefaz.Facades;
using System;
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

            string xml = @"<retEnviNFe versao='4.00' xmlns='http://www.portalfiscal.inf.br/nfe'>
		                    <tpAmb>2</tpAmb>
		                    <verAplic>SVRSnfce202103291658</verAplic>
		                    <cStat>104</cStat>
		                    <xMotivo>Lote processado</xMotivo>
		                    <cUF>53</cUF>
		                    <dhRecbto>2021-07-05T18:30:42-03:00</dhRecbto>
		                    <protNFe versao= '4.00'>
		                        <infProt>
		                            <tpAmb>2</tpAmb>
		                            <verAplic>SVRSnfce202103291658</verAplic>
		                            <chNFe>53210704585789000140650030000006141325009918</chNFe>
		                            <dhRecbto>2021-07-05T18:30:42-03:00</dhRecbto>
		                            <nProt>353210000029778</nProt>
		                            <digVal>d/kQ4t6qpkXw1i/JQDXstFfcbH0=</digVal>
		                            <cStat>100</cStat>
		                            <xMotivo>Autorizado o uso da NF-e</xMotivo>
		                        </infProt>
		                    </protNFe>
		                </retEnviNFe>";

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xml);

            nfeAutorizacaoSoapMock.Setup(n => n.nfeAutorizacaoLote(It.IsAny<nfeAutorizacaoLoteRequest>()))
                .Returns(new nfeAutorizacaoLoteResponse() { nfeResultMsg = xmlDocument });

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
    }
}
