using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using NFe.Core.NfeStatusServico4;
using NFe.Core.Domain;
using NFe.Core.Sefaz;
using NFe.Core.Utils.Conversores.Enums.StatusServico;
using NFe.Core.XMLSchemas.NfeStatusServico2.Envio;

namespace NFe.Core.NotasFiscais.Sefaz.NfeStatusServico2
{
    public class NFeStatusServicoService
    {
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public bool ConsultarStatus(CodigoUfIbge codigoUF, Ambiente ambiente, X509Certificate2 certificado, Modelo modelo)
        {
            try
            {
                var parametro = new TConsStatServ
                {
                    cUF = TCodUfIBGEConversor.ToTCodUfIBGE(codigoUF),
                    tpAmb = ambiente == Ambiente.Homologacao ? TAmb.Item2 : TAmb.Item1,
                    versao = "4.00",
                    xServ = TConsStatServXServ.STATUS
                };

                const string nFeNamespaceName = "http://www.portalfiscal.inf.br/nfe";
                string parametroXML = XmlUtil.Serialize(parametro, nFeNamespaceName);

                XmlDocument doc = new XmlDocument();
                XmlReader reader = XmlReader.Create(new StringReader(parametroXML));
                reader.MoveToContent();

                XmlNode node = doc.ReadNode(reader);
                string endpoint = "";

                if (modelo == Modelo.Modelo55)
                {
                    endpoint = "NfeStatusServico2";
                }
                else
                {
                    endpoint = "NfceStatusServico2";
                }

                var soapClient = new NfeStatusServico4SoapClient(endpoint);
                soapClient.ClientCredentials.ClientCertificate.Certificate = certificado;

                XmlNode result = soapClient.nfeStatusServicoNF(node);

                var retorno = (XmlSchemas.NfeStatusServico2.Retorno.TRetConsStatServ)XmlUtil.Deserialize<XmlSchemas.NfeStatusServico2.Retorno.TRetConsStatServ>(result.OuterXml);

                return retorno.cStat == "107";
            }
            catch (Exception e)
            {
                log.Error(e);
                return false;
            }
        }
    }

}
