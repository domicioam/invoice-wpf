using NFe.Core.Domain;
using NFe.Core.NFeConsultaProtocolo4;
using NFe.Core.Sefaz;
using NFe.Core.XmlSchemas.NfeConsulta2.Envio;
using NFe.Core.XmlSchemas.NfeConsulta2.Retorno;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using Envio = NFe.Core.XmlSchemas.NfeConsulta2.Envio;

namespace NFe.Core.NotasFiscais.Sefaz.NfeConsulta2
{
    public class ConsultarNotaFiscalService : IConsultarNotaFiscalService
    {
        private readonly SefazSettings sefazSettings;

        public ConsultarNotaFiscalService(SefazSettings sefazSettings)
        {
            this.sefazSettings = sefazSettings;
        }

        public RetornoConsulta ConsultarNotaFiscal(string chave, string codigoUf, X509Certificate2 certificado, Modelo modelo)
        {
            string endpoint;
            if (modelo == Modelo.Modelo55)
                endpoint = "NfeConsulta2";
            else
                endpoint = "NfceConsulta2";

            string nFeNamespaceName = "http://www.portalfiscal.inf.br/nfe";
            var parametro = new TConsSitNFe
            {
                chNFe = chave,
                tpAmb = sefazSettings.Ambiente == Ambiente.Homologacao ? Envio.TAmb.Item2 : Envio.TAmb.Item1,
                versao = Envio.TVerConsSitNFe.Item400,
                xServ = TConsSitNFeXServ.CONSULTAR
            };
            string parametroXML = XmlUtil.Serialize(parametro, nFeNamespaceName);
            XmlReader reader = XmlReader.Create(new StringReader(parametroXML));
            reader.MoveToContent();
            XmlDocument doc = new XmlDocument();
            XmlNode node = doc.ReadNode(reader);

            TRetConsSitNFe retornoConsulta = ExecutaConsulta(certificado, endpoint, node);

            if (retornoConsulta.cStat != "100") return new RetornoConsulta { IsEnviada = false };

            return new RetornoConsulta
            {
                IsEnviada = true,
                Protocolo = new Protocolo(retornoConsulta.protNFe),
                DhAutorizacao = retornoConsulta.protNFe.infProt.dhRecbto //não é a hora da autorização
            };
        }

        protected virtual TRetConsSitNFe ExecutaConsulta(X509Certificate2 certificado, string endpoint, XmlNode node)
        {
            var soapClient = new NFeConsultaProtocolo4SoapClient(endpoint);
            soapClient.ClientCredentials.ClientCertificate.Certificate = certificado;
            XmlNode result = soapClient.nfeConsultaNF(node);
            return (TRetConsSitNFe)XmlUtil.Deserialize<TRetConsSitNFe>(result.OuterXml);
        }
    }
}
