using DgSystem.NFe.Reports.Nfce;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DgSystem.NFe.Reports.Nfe
{
    public class ReportNFeReadModel
    {
        public string Chave { get; set; }
        public string Numero { get; set; }
        public string Serie { get; set; }
        public DateTime DataHoraEmissao { get; set; }
        public string Protocolo { get; set; }
        public string DataHoraAutorizacao { get; set; }
        public string InformacaoAdicional { get; set; }
        public string LinkConsultaChave { get; set; }
        public string InformacaoInteresse { get; set; }
        public int QuantidadeTotalProdutos { get; set; }
        public double ValorTotalProdutos { get; set; }
        public byte[] QrCodeImage { get; set; }
        public Emitente Emissor { get; set; }
        public Destinatario Destinatario { get; set; }
        public IEnumerable<Produto> Produtos { get; set; }
        public IEnumerable<Pagamento> Pagamentos { get; set; }
        public List<ItemTotal> TotaisNotaFiscal { get; set; }
        public string DataSaida { get; internal set; }
        public string HoraSaida { get; internal set; }
        public string NaturezaOperacao { get; internal set; }
        public string TipoOperacao { get; internal set; }
        public byte[] BarcodeImage { get; internal set; }
        public Transportadora Transportadora { get; internal set; }
        public CalculoImposto CalculoImposto { get; internal set; }
    }
}
