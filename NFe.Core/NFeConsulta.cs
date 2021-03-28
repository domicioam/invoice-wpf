using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using NFe.Core.NFeConsultaProtocolo4;
using NFe.Core.Domain;
using NFe.Core.Sefaz;
using NFe.Core.XmlSchemas.NfeConsulta2.Envio;
using NFe.Core.XmlSchemas.NfeConsulta2.Retorno;
using Envio = NFe.Core.XmlSchemas.NfeConsulta2.Envio;

namespace NFe.Core.NotasFiscais.Sefaz.NfeConsulta2
{
    public class NFeConsulta : IConsultarNotaFiscalService
    {
        private SefazSettings _sefazSettings;

        public MensagemRetornoConsulta ConsultarNotaFiscal(string chave, string codigoUf, X509Certificate2 certificado, Modelo modelo)
      {
         XmlNode node = null;

         var parametro = new TConsSitNFe();
         parametro.chNFe = chave;
         parametro.tpAmb = _sefazSettings.Ambiente == Ambiente.Homologacao ? Envio.TAmb.Item2 : Envio.TAmb.Item1;
         parametro.versao = Envio.TVerConsSitNFe.Item400;
         parametro.xServ = TConsSitNFeXServ.CONSULTAR;

         string nFeNamespaceName = "http://www.portalfiscal.inf.br/nfe";
         string parametroXML = XmlUtil.Serialize(parametro, nFeNamespaceName);

         XmlDocument doc = new XmlDocument();
         XmlReader reader = XmlReader.Create(new StringReader(parametroXML));
         reader.MoveToContent();

         node = doc.ReadNode(reader);

         string endpoint = "";

        if (modelo == Modelo.Modelo55)
        {
            endpoint = "NfeConsulta2";
        }
        else
        {
            endpoint = "NfceConsulta2";
        }
         

         var soapClient = new NFeConsultaProtocolo4SoapClient(endpoint);
         soapClient.ClientCredentials.ClientCertificate.Certificate = certificado;
         XmlNode result = soapClient.nfeConsultaNF(node);

         var retornoConsulta = (TRetConsSitNFe)XmlUtil.Deserialize<TRetConsSitNFe>(result.OuterXml);

         MensagemRetornoConsulta mensagemRetorno = new MensagemRetornoConsulta();

         if (retornoConsulta.cStat == "100")
         {
            mensagemRetorno.IsEnviada = true;
            mensagemRetorno.Protocolo = retornoConsulta.protNFe;
            mensagemRetorno.DhAutorizacao = retornoConsulta.protNFe.infProt.dhRecbto; //não é a hora da autorização
         }
         else
         {
            mensagemRetorno.IsEnviada = false;
         }

         return mensagemRetorno;
      }

        public NFeConsulta(SefazSettings sefazSettings)
        {
            _sefazSettings = sefazSettings;
        }

      public struct MensagemRetornoConsulta
      {
         public bool IsEnviada { get; set; }
         public DateTime DhAutorizacao { get; set; }
         public TProtNFe Protocolo { get; set; }
      }
   }
}
