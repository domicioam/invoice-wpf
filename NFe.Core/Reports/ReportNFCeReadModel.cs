using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFe.WPF.Model
{
    public class ReportNFCeReadModel
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
        public Emissor Emissor { get; set; }
        public List<Produto> Produtos { get; set; }
        public List<Pagamento> Pagamentos { get; set; }
        public List<ItemTotal> TotaisNotaFiscal { get; set; }
        public int QuantidadeTotalProdutos { get; set; }
        public double ValorTotalProdutos  { get; set; }
        public Destinatario Destinatario { get; set; }
        public byte[] QrCodeImage { get; set; }
    }

    [Serializable]
    public struct Emissor
    {
        public string CNPJ { get; set; }
        public string Nome { get; set; }
        public string Logradouro { get; set; }
        public string Numero { get; set; }
        public string Bairro { get; set; }
        public string Municipio { get; set; }
        public string UF { get; set; }
        public string CEP { get; set; }
    }

    [Serializable]
    public struct Produto
    {
        public string Codigo { get; set; }
        public string Descricao { get; set; }
        public int Quantidade { get; set; }
        public double ValorUnitario { get; set; }
        public double ValorTotal { get; set; }
    }

    [Serializable]
    public struct Pagamento
    {
        public string Nome { get; set; }
        public double Valor { get; set; }
    }

    [Serializable]
    public struct Destinatario
    {
        public string Nome { get; set; }
        public string Documento { get; set; }
        public string Logradouro { get; set; }
        public string Numero { get; set; }
        public string Bairro { get; set; }
        public string Municipio { get; set; }
        public string UF { get; set; }
        public string CEP { get; set; }

    }

    [Serializable]
    public struct ItemTotal
    {
        public string Descricao { get; set; }
        public double Valor { get; set; }
    }
}
