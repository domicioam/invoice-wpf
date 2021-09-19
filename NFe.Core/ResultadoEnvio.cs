using NFe.Core.Domain;
using NFe.Core.XmlSchemas.NfeAutorizacao.Envio;
using NFe.Core.XmlSchemas.NfeAutorizacao.Retorno;
using System.Xml;

namespace NFe.Core.Sefaz.Facades
{
    public class ResultadoEnvio
    {
        public ResultadoEnvio(NotaFiscal notaFiscal, TProtNFe protocolo, QrCode qrCode, TNFe nfe, XmlNode node)
        {
            NotaFiscal = notaFiscal;
            Protocolo = protocolo;
            QrCode = qrCode;
            Nfe = nfe;
            Node = node;
        }

        public NotaFiscal NotaFiscal { get; }
        public TProtNFe Protocolo { get; }
        public QrCode QrCode { get; }
        public TNFe Nfe { get; }
        public XmlNode Node { get; }
    }
}
