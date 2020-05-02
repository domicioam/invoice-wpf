using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using NFe.Core.NfeStatusServico4;
using NFe.Core.Utils.Conversores.Enums.StatusServico;
using NFe.Core.Utils.Xml;
using NFe.Core.XMLSchemas.NfeStatusServico2.Envio;

namespace NFe.Core.NotasFiscais.Sefaz.NfeStatusServico2
{
    public static class NFeStatusServico
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static bool ExecutarConsultaStatus(CodigoUfIbge codigoUF, Ambiente ambiente, X509Certificate2 certificado, Modelo modelo)
        {
            XmlNode node = null;

            try
            {
                var parametro = new TConsStatServ();
                parametro.cUF = TCodUfIBGEConversor.ToTCodUfIBGE(codigoUF);
                parametro.tpAmb = ambiente == Ambiente.Homologacao ? TAmb.Item2 : TAmb.Item1;
                parametro.versao = "4.00";
                parametro.xServ = TConsStatServXServ.STATUS;

                string nFeNamespaceName = "http://www.portalfiscal.inf.br/nfe";
                string parametroXML = XmlUtil.Serialize(parametro, nFeNamespaceName);

                XmlDocument doc = new XmlDocument();
                XmlReader reader = XmlReader.Create(new StringReader(parametroXML));
                reader.MoveToContent();

                node = doc.ReadNode(reader);

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
