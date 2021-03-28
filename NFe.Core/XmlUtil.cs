using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Xml.Serialization;
using System.Collections.Generic;

using NFe.Core.Domain;
using NFe.Core.Sefaz.Facades;
using NFe.Core.XmlSchemas.NfeAutorizacao.Envio;
using NFe.Core.XmlSchemas.NfeAutorizacao.Retorno.NfeProc;
using TNFe = NFe.Core.XmlSchemas.NfeAutorizacao.Envio.TNFe;
using TProtNFe = NFe.Core.XmlSchemas.NfeAutorizacao.Retorno.TProtNFe;

/** O método para pegar a nota fiscal a partir do xml encontra-se em NFeAutorizacaoNormal.GetNotaFiscalFromNfeProcXML() **/

namespace NFe.Core.Sefaz
{
    public class XmlUtil
    {
        public static string Serialize<T>(T value, string namespaceName)
        {
            if (value == null) throw new ArgumentNullException();

            var xmlSerializer = new XmlSerializer(typeof(T));
            var xsn = new XmlSerializerNamespaces();
            xsn.Add("", namespaceName);

            using (var stream = new MemoryStream())
            {
                using (var writer = new XmlTextWriter(stream, new UTF8Encoding(false)))
                {
                    xmlSerializer.Serialize(writer, value, xsn);
                    var streamToRead = (MemoryStream)writer.BaseStream;
                    var encoding = new UTF8Encoding();
                    return encoding.GetString(streamToRead.ToArray());
                }
            }
        }

        public static object Deserialize<T>(string xml)
        {
            if (xml == null) throw new ArgumentNullException();

            var xmlSerializer = new XmlSerializer(typeof(T));

            var encoding = new UTF8Encoding();
            var byteArray = encoding.GetBytes(xml);

            using (var stream = new MemoryStream(byteArray))
            {
                return xmlSerializer.Deserialize(stream);
            }
        }

        public virtual string GerarNfeProcXml(TNFe nfe, QrCode urlQrCode, TProtNFe protocolo = null)
        {
            var nfeProc = new TNfeProc();
            var nFeNamespaceName = "http://www.portalfiscal.inf.br/nfe";

            nfeProc.NFe = nfe.ToTNFeRetorno(nFeNamespaceName);

            if (nfeProc.NFe.infNFeSupl != null) nfeProc.NFe.infNFeSupl.qrCode = "";

            if (protocolo != null)
            {
                var protocoloSerializado = Serialize(protocolo, nFeNamespaceName);
                nfeProc.protNFe = (XmlSchemas.NfeAutorizacao.Retorno.NfeProc.TProtNFe) Deserialize<XmlSchemas.NfeAutorizacao.Retorno.NfeProc.TProtNFe>(protocoloSerializado);
            }
            else
            {
                nfeProc.protNFe = new XmlSchemas.NfeAutorizacao.Retorno.NfeProc.TProtNFe();
            }

            nfeProc.versao = "4.00";
            var result = Serialize(nfeProc, nFeNamespaceName).Replace("<motDesICMS>1</motDesICMS>", string.Empty);

            if (nfeProc.NFe.infNFeSupl != null)
                result = result.Replace("<qrCode />", "<qrCode>" + urlQrCode + "</qrCode>")
                    .Replace("<NFe>", "<NFe xmlns=\"http://www.portalfiscal.inf.br/nfe\">");
            else
                result = result.Replace("<NFe>", "<NFe xmlns=\"http://www.portalfiscal.inf.br/nfe\">");

            return result;
        }



        public static string GerarXmlLoteNFe(NotaFiscal notaFiscal, string nFeNamespaceName)
        {
            TEnviNFe lote = ModelToSefazAdapter.GetLoteNFe(notaFiscal);

            var parametroXml = Serialize(lote, nFeNamespaceName);
            return parametroXml.Replace("<NFe>", "<NFe xmlns=\"http://www.portalfiscal.inf.br/nfe\">");
        }

        public static string GerarXmlListaNFe(List<string> notasFiscais)
        {
            var notasConcatenadas = new StringBuilder();

            for (var i = 0; i < notasFiscais.Count; i++)
            {
                var nfeProc = new XmlDocument();
                nfeProc.LoadXml(notasFiscais[i]);
                notasConcatenadas.Append(nfeProc.GetElementsByTagName("NFe")[0].OuterXml);
            }

            return notasConcatenadas.ToString();
        }
    }
}