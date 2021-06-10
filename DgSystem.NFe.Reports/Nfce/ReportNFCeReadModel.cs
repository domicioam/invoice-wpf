using System;
using System.Collections.Generic;

namespace DgSystem.NFe.Reports.Nfce
{
    internal class ReportNFCeReadModel
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
        public List<Produto> Produtos { get; set; }
        public IEnumerable<Pagamento> Pagamentos { get; set; }
        public List<ItemTotal> TotaisNotaFiscal { get; set; }
    }
}