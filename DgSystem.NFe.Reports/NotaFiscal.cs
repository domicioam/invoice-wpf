using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DgSystem.NFe.Reports
{
    [Serializable]
    public class NotaFiscal
    {
        public NotaFiscal(string qrCodeUrl, Identificacao identificacao, Emitente emitente, Destinatario destinatario, IEnumerable<Produto> produtos, IEnumerable<Pagamento> pagamentos, double valorTotalProdutos, string protocoloAutorizacao, string dhAutorizacao, string infoAdicionalComplementar, int qtdTotalProdutos)
        {
            QrCodeUrl = qrCodeUrl;
            Identificacao = identificacao;
            Emitente = emitente;
            Destinatario = destinatario;
            Produtos = produtos;
            Pagamentos = pagamentos;
            ValorTotalProdutos = valorTotalProdutos;
            ProtocoloAutorizacao = protocoloAutorizacao;
            DhAutorizacao = dhAutorizacao;
            InfoAdicionalComplementar = infoAdicionalComplementar;
            QtdTotalProdutos = qtdTotalProdutos;
        }

        public string QrCodeUrl { get; internal set; }
        public Identificacao Identificacao { get; internal set; }
        public Emitente Emitente { get; internal set; }
        public Destinatario Destinatario { get; internal set; }
        public IEnumerable<Produto> Produtos { get; internal set; }
        public IEnumerable<Pagamento> Pagamentos { get; internal set; }
        public double ValorTotalProdutos { get; internal set; }
        public string ProtocoloAutorizacao { get; internal set; }
        public string DhAutorizacao { get; internal set; }
        public string InfoAdicionalComplementar { get; internal set; }
        public int QtdTotalProdutos { get; internal set; }
    }
}
