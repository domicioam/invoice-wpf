using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using DgSystems.NFe.Core.Cadastro;
using NFe.Core.Cadastro.Certificado;
using NFe.Core.Cadastro.Ibpt;
using NFe.Core.Cadastro.Imposto;
using NFe.Core.Domain;

namespace DgSystems.NFe.Core.UnitTests
{
    public class NotaFiscalFixture
    {
        public NotaFiscalFixture()
        {
            var data = new DateTime(2020, 10, 20);
            var endereçoEmitente = new Endereco("QUADRA 200 CONJUNTO 20", "20", "BRASILIA", "BRASILIA", "70000000", "DF");
            
            var emitente = new Emissor("RAZAO SOCIAL", "NOME FANTASIA", "12345678998765", "1234567898765",
                "1234567898765", "4784900", "Regime Normal", endereçoEmitente, "99999999");
            
            var identificação = new IdentificacaoNFe(CodigoUfIbge.DF, data, emitente.CNPJ, Modelo.Modelo65, 1,
                "20887", TipoEmissao.Normal, Ambiente.Homologacao, emitente.Endereco.CodigoMunicipio, "Venda", FinalidadeEmissao.Normal, true,
                PresencaComprador.Presencial, "CPF");
            
            var transporte = new Transporte(Modelo.Modelo65, null, null);
            const int valorTotalProdutos = 65;
            var totalIcms = new IcmsTotal(0, 0, 0, 0, 0, valorTotalProdutos, 0, 0, 0, 0, 0, 0, 0, 0, valorTotalProdutos,
                0);
            var totalNFe = new TotalNFe { IcmsTotal = totalIcms };
            var impostosList = new List<global::NFe.Core.Domain.Imposto>
                {
                    new global::NFe.Core.Domain.Imposto {CST = "60", TipoImposto = TipoImposto.Icms},
                    new global::NFe.Core.Domain.Imposto {CST = "04", TipoImposto = TipoImposto.PIS}
                };

            var impostos = new Impostos(impostosList);

            var produtos = new List<Produto>
                {
                    new Produto(impostos, 0, "5656", "0001", "GLP 13KG", "27111910", 1, "UN", 65, 0, false,0,0,0)
                };
            var pagamentos = new List<Pagamento>
                {
                    new Pagamento(FormaPagamento.Dinheiro) {Valor = 65}
                };
            var infoAdicional = new InfoAdicional(produtos, new IbptManager());
            var notaFiscal = new NotaFiscal(emitente, null, identificação, transporte, totalNFe, infoAdicional,
                produtos, pagamentos);

            NotaFiscal = notaFiscal;
        }


        public string NfeNamespaceName => "http://www.portalfiscal.inf.br/nfe";
        public string CscId => "000001";
        public string Csc => "E3BB2129-7ED0-31A10-CCB8-1B8BAC8FA2D0";
        public CertificadoEntity CertificadoEntity
        {
            get
            {
                return new CertificadoEntity
                {
                    Caminho = "MyDevCert.pfx",
                    Nome = "MOCK NAME",
                    NumeroSerial = "1234",
                    Senha = "VqkVinLLG4/EAKUokpnVDg=="
                };
            }
        }

        public X509Certificate2 X509Certificate2
        {
            get
            {
                return new X509Certificate2("MyDevCert.pfx", "SuperS3cret!");
            }
        }

        public NotaFiscal NotaFiscal
        {
            get;
            private set;
        }
    }
}
