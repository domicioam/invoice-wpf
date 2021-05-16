using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DgSystem.NFe.Reports
{

    internal class NotaFiscal
    {
        public string QrCodeUrl { get; internal set; }
        public Identificacao Identificacao { get; internal set; }
        public Emitente Emitente { get; internal set; }
        public Destinatario Destinatario { get; internal set; }
        public IEnumerable<Produto> Produtos { get; internal set; }
        public IEnumerable<Pagamento> Pagamentos { get; internal set; }
        public double ValorTotalProdutos { get; internal set; }
        public string ProtocoloAutorizacao { get; internal set; }
        public string DhAutorizacao { get; internal set; }
        public object InfoAdicionalComplementar { get; internal set; }
        public object QtdTotalProdutos { get; internal set; }
    }
}
