using MediatR;
using Moq;
using NFe.Core.Cadastro.Certificado;
using NFe.Core.Cadastro.Configuracoes;
using NFe.Core.Cadastro.Emissor;
using NFe.Core.Cadastro.Imposto;
using NFe.Core.Entitities;
using NFe.Core.Events;
using NFe.Core.Interfaces;
using NFe.Core.NFeAutorizacao4;
using NFe.Core.NotasFiscais;
using NFe.Core.NotasFiscais.Sefaz.NfeConsulta2;
using NFe.Core.NotasFiscais.Services;
using NFe.Core.Sefaz.Facades;
using NFe.Core.Utils.Assinatura;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Xunit;

namespace NFe.Core.UnitTests.NotaFiscalService
{
    public class NotaFiscalServiceTest
    {
        [Fact]
        public void NotaFiscalService_EnviarNotaFiscal_Sucesso()
        {

        }

        private static void ConfigurarConfiguracao()
        {
            var configuracao = new ConfiguracaoEntity
            {
                Id = 1,
                SerieNFCe = "001",
                SerieNFe = "001",
                ProximoNumNFCe = "1",
                ProximoNumNFe = "1",
            };

            var configuracaoServiceMock = new Mock<IConfiguracaoService>();
            configuracaoServiceMock.Setup(m => m.GetConfiguracao()).Returns(configuracao);
        }

        private static Mock<IServiceFactory> ConfigurarServiceFactoryMock()
        {
            var nfeAutorizacao4Mock = new Mock<NFeAutorizacao4Soap>();
            nfeAutorizacao4Mock
                .Setup(m => m.nfeAutorizacaoLote(It.IsAny<nfeAutorizacaoLoteRequest>()))
                .Throws(new WebException());

            var serviceFactoryMock = new Mock<IServiceFactory>();
            serviceFactoryMock
                .Setup(m => m.GetService(It.IsAny<Modelo>(), It.IsAny<Servico>(),
                    It.IsAny<CodigoUfIbge>(), It.IsAny<X509Certificate2>())).Returns(() =>
                    {
                        return new Service { SoapClient = nfeAutorizacao4Mock.Object };
                    });
            return serviceFactoryMock;
        }

        private static void ConfigurarCertificadoDigital(out Mock<ICertificadoRepository> certificadoRepositoryMock, out Mock<ICertificateManager> certificadoManagerMock)
        {
            certificadoRepositoryMock = new Mock<ICertificadoRepository>();
            certificadoRepositoryMock
                .Setup(m => m.GetCertificado())
                .Returns(() => new CertificadoEntity
                {
                    Caminho = "MyDevCert.pfx",
                    Nome = "MOCK NAME",
                    NumeroSerial = "1234",
                    Senha = "VqkVinLLG4/EAKUokpnVDg=="
                });

            var cert = new X509Certificate2("MyDevCert.pfx", "SuperS3cret!");
            certificadoManagerMock = new Mock<ICertificateManager>();
            certificadoManagerMock
                .Setup(m => m.GetCertificateByPath(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(() => cert);
        }

        private static NotaFiscal PreencherNotaFiscal()
        {
            var endereçoEmitente =
                new Endereco("QUADRA 200 CONJUNTO 20", "20", "BRASILIA", "BRASILIA", "70000000", "DF");
            var emitente = new Emissor("RAZAO SOCIAL", "NOME FANTASIA", "12345678998765", "1234567898765",
                "1234567898765", "4784900", "Regime Normal", endereçoEmitente, "99999999");
            var identificação = new IdentificacaoNFe(CodigoUfIbge.DF, DateTime.Now, emitente.CNPJ, Modelo.Modelo65, 1,
                "20887", TipoEmissao.Normal, Ambiente.Homologacao, emitente, "Venda", FinalidadeEmissao.Normal, true,
                PresencaComprador.Presencial, "CPF");
            var transporte = new Transporte(Modelo.Modelo65, null, null);
            const int valorTotalProdutos = 65;
            var totalIcms = new IcmsTotal(0, 0, 0, 0, 0, valorTotalProdutos, 0, 0, 0, 0, 0, 0, 0, 0, valorTotalProdutos,
                0);
            var totalNFe = new TotalNFe { IcmsTotal = totalIcms };
            var impostos = new List<Imposto>
            {
                new Imposto {CST = "60", TipoImposto = TipoImposto.Icms},
                new Imposto {CST = "04", TipoImposto = TipoImposto.PIS}
            };
            var grupoImpostos = new GrupoImpostos
            {
                CFOP = "5656",
                Descricao = "Gás Venda",
                Impostos = impostos
            };
            var produtos = new List<Produto>
            {
                new Produto(grupoImpostos, 0, "5656", "0001", "GLP 13KG", "27111910", 1, "UN", 65, 0, false)
            };
            var pagamentos = new List<Pagamento>
            {
                new Pagamento(FormaPagamento.Dinheiro) {Valor = 65}
            };
            var infoAdicional = new InfoAdicional(produtos);
            var notaFiscal = new NotaFiscal(emitente, null, identificação, transporte, totalNFe, infoAdicional,
                produtos, pagamentos);
            return notaFiscal;
        }
    }
}