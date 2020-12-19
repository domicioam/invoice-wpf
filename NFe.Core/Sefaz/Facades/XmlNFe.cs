using NFe.Core.NotasFiscais;
using NFe.Core.Utils.Assinatura;
using NFe.Core.XmlSchemas.NfeAutorizacao.Envio;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace NFe.Core.Sefaz.Facades
{
    public class XmlNFe
    {
        public XmlNFe(NotaFiscal notaFiscal, string nfeNamespace, X509Certificate2 certificado, string cscId, string csc)
        {
            var refUri = "#NFe" + notaFiscal.Identificacao.Chave;
            var digVal = "";

            var xml = Regex.Replace(XmlUtil.GerarXmlLoteNFe(notaFiscal, nfeNamespace), "<motDesICMS>1</motDesICMS>", string.Empty);
            XmlNode = AssinaturaDigital.AssinarLoteComUmaNota(xml, refUri, certificado, ref digVal);

            if (notaFiscal.Identificacao.Modelo == Modelo.Modelo65)
            {
                QrCode = PreencherQrCode(notaFiscal, cscId, csc, digVal);
                string newNodeXml = XmlNode.InnerXml.Replace("<qrCode />", "<qrCode>" + QrCode + "</qrCode>");
                var document = new XmlDocument();
                document.LoadXml(newNodeXml);
                XmlNode = document.DocumentElement;
            }

            var lote = (TEnviNFe)XmlUtil.Deserialize<TEnviNFe>(XmlNode.OuterXml);
            TNFe = lote.NFe[0];
        }

        public XmlNode XmlNode { get; private set; }
        public TNFe TNFe { get; private set; }
        public QrCode QrCode { get; private set; }
        public string Xml { get { return XmlNode.OuterXml; } }

        private static QrCode PreencherQrCode(NotaFiscal notaFiscal, string cscId, string csc, string digVal)
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
