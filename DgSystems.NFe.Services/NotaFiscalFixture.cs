using DgSystems.NFe.Core.Cadastro;
using NFe.Core.Cadastro.Certificado;
using NFe.Core.Cadastro.Ibpt;
using NFe.Core.Domain;
using NFe.Core.XmlSchemas.NfeAutorizacao.Retorno;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Xml;

namespace DgSystems.NFe.Core.UnitTests
{
    public class NotaFiscalFixture
    {
        public const string DATE_STRING_FORMAT = "dd/MM/yyyy HH:mm:ss";
        public const string DATE_STRING_SEFAZ_FORMAT = "yyyy-MM-ddTHH:mm:sszzz";

        public NotaFiscalFixture()
        {
            NotaFiscal = CreateNotaFiscal();
        }

        public TProtNFe ProtNFe => new TProtNFe()
        {
            infProt = new TProtNFeInfProt()
            {
                cStat = "100",
                dhRecbto = new DateTime(20, 10, 20).ToString(DATE_STRING_SEFAZ_FORMAT),
                nProt = "12345"
            }
        };

        public XmlDocument nfeResultMsg
        {
            get
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(Xml);
                return xmlDocument;
            }
        }

        public string Xml => @"<retEnviNFe versao='4.00' xmlns='http://www.portalfiscal.inf.br/nfe'>
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


        private NotaFiscal CreateNotaFiscal()
        {
            var endereçoEmitente = new Endereco("QUADRA 200 CONJUNTO 20", "20", "BRASILIA", "BRASILIA", "70000000", "DF");
            var emitente = new Emissor("RAZAO SOCIAL", "NOME FANTASIA", "12345678998765", "1234567898765",
                "1234567898765", "4784900", "Regime Normal", endereçoEmitente, "99999999");

            var identificação = new IdentificacaoNFe(CodigoUfIbge.DF, new DateTime(2020, 10, 20), emitente.CNPJ, Modelo.Modelo65, 1,
                "20887", TipoEmissao.Normal, Ambiente.Homologacao, emitente.Endereco.CodigoMunicipio, "Venda", FinalidadeEmissao.Normal, true,
                PresencaComprador.Presencial, "CPF");

            var (impostos, totalNFe) = CreateImpostos();

            var produtos = new List<Produto> { new Produto(impostos, 0, "5656", "0001", "GLP 13KG", "27111910", 1, "UN", 65, 0, false, 0, 0, 0) };
            var infoAdicional = new InfoAdicional(produtos, new IbptManager());
            var transporte = new Transporte(Modelo.Modelo65, null, null);
            var pagamentos = new List<Pagamento> { new Pagamento(FormaPagamento.Dinheiro) { Valor = 65 } };
            return new NotaFiscal(emitente, null, identificação, transporte, totalNFe, infoAdicional,
                produtos, pagamentos);
        }

        private (Impostos, TotalNFe) CreateImpostos()
        {
            const int valorTotalProdutos = 65;
            var totalIcms = new IcmsTotal(0, 0, 0, 0, 0, valorTotalProdutos, 0, 0, 0, 0, 0, 0, 0, 0, valorTotalProdutos, 0);
            var totalNFe = new TotalNFe { IcmsTotal = totalIcms };
            var impostosList = new List<global::NFe.Core.Domain.Imposto>
                {
                    new global::NFe.Core.Domain.Imposto {CST = "60", TipoImposto = TipoImposto.Icms},
                    new global::NFe.Core.Domain.Imposto {CST = "04", TipoImposto = TipoImposto.PIS}
                };

            return (new Impostos(impostosList), totalNFe);
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
