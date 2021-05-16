using System.Collections.Generic;

namespace DgSystem.NFe.Reports
{
    internal class ReportNFCeReadModel
    {
        public object Chave { get; set; }
        public object Numero { get; set; }
        public object Serie { get; set; }
        public object DataHoraEmissao { get; set; }
        public object Protocolo { get; set; }
        public object DataHoraAutorizacao { get; set; }
        public object InformacaoAdicional { get; set; }
        public object LinkConsultaChave { get; set; }
        public object InformacaoInteresse { get; set; }
        public object QuantidadeTotalProdutos { get; set; }
        public double ValorTotalProdutos { get; set; }
        public byte[] QrCodeImage { get; set; }
        public object Emissor { get; set; }
        public object Destinatario { get; set; }
        public List<Produto> Produtos { get; set; }
        public object Pagamentos { get; set; }
        public List<ItemTotal> TotaisNotaFiscal { get; set; }
    }
}