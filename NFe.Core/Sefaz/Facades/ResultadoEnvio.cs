using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using NFe.Core.NotasFiscais;
using NFe.Core.XmlSchemas.NfeAutorizacao.Envio;
using NFe.Core.XmlSchemas.NfeAutorizacao.Retorno;

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
