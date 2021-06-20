using NFe.Core.Domain;
using NFe.Core.Utils.Assinatura;
using NFe.Core.XmlSchemas.NfeAutorizacao.Envio;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Xml;

namespace NFe.Core.Sefaz.Facades
{
    public class XmlNFe
    {
        public XmlNFe(Domain.NotaFiscal notaFiscal, string nfeNamespace, X509Certificate2 certificado, string cscId, string csc)
        {
            var refUri = "#NFe" + notaFiscal.Identificacao.Chave;
            var digVal = "";

            var xml = Regex.Replace(XmlUtil.GerarXmlLoteNFe(notaFiscal, nfeNamespace), "<motDesICMS>1</motDesICMS>", string.Empty);
            var xmlNode = AssinaturaDigital.AssinarLoteComUmaNota(xml, refUri, certificado, ref digVal);
            XmlDocument = new XmlDocument();
            XmlDocument.LoadXml(xmlNode.InnerXml);

            if (notaFiscal.Identificacao.Modelo == Modelo.Modelo65)
            {
                QrCode = PreencherQrCode(notaFiscal, cscId, csc, digVal);
                string newNodeXml = xmlNode.InnerXml.Replace("<qrCode />", "<qrCode>" + QrCode + "</qrCode>");
                XmlDocument = new XmlDocument();
                XmlDocument.LoadXml(newNodeXml);
            }

            var lote = (TEnviNFe)XmlUtil.Deserialize<TEnviNFe>(xmlNode.OuterXml);
            TNFe = lote.NFe[0];
        }

        public XmlNode XmlNode
        {
            get { return XmlDocument.DocumentElement; }
        }

        public XmlDocument XmlDocument { get; }
        public TNFe TNFe { get; }
        public QrCode QrCode { get; }
        public string Xml { get { return XmlDocument.OuterXml; } }

        private static QrCode PreencherQrCode(Domain.NotaFiscal notaFiscal, string cscId, string csc, string digVal)
        {
            QrCode qrCode = new QrCode();

            qrCode.GerarQrCodeNFe(notaFiscal.Identificacao.Chave, notaFiscal.Destinatario,
               digVal, notaFiscal.Identificacao.Ambiente,
               notaFiscal.Identificacao.DataHoraEmissao,
               notaFiscal.GetTotal().ToString("F", CultureInfo.InvariantCulture), notaFiscal.GetTotalIcms().ToString("F", CultureInfo.InvariantCulture), cscId, csc, notaFiscal.Identificacao.TipoEmissao);

            return qrCode;
        }
    }
}
