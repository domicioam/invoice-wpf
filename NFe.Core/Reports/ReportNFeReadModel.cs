using System;
using System.Collections.Generic;

namespace NFe.WPF.Model.ReportNFe
{
    public class ReportNFeReadModel
    {
        public string Chave { get; set; }
        public string Numero { get; set; }
        public string Serie { get; set; }
        public DateTime DataHoraEmissao { get; set; }
        public string ProtocoloAutorizacao { get; set; }
        public string NaturezaOperacao { get; set; }
        public string DataHoraAutorizacao { get; set; }
        public string DataSaida { get; set; }
        public string HoraSaida { get; set; }
        public string InformacaoAdicional { get; set; }
        public string LinkConsultaChave { get; set; }
        public string InformacaoInteresse { get; set; }
        public string TipoOperacao { get; set; }
        public Emissor Emissor { get; set; }
        public List<Produto> Produtos { get; set; }
        public List<Pagamento> Pagamentos { get; set; }
        public int QuantidadeTotalProdutos { get; set; }
        public double ValorTotalProdutos { get; set; }
        public Destinatario Destinatario { get; set; }
        public CalculoImposto CalculoImposto { get; set; }
        public Transportadora Transportadora { get; set; }
        public byte[] BarcodeImage { get; set; }
    }

    [Serializable]
    public struct Emissor
    {
        public string CNPJ { get; set; }
        public string InscricaoEstadual { get; set; }
        public string NomeRazao { get; set; }
        public string NomeFantasia { get; set; }
        public string Logradouro { get; set; }
        public string Numero { get; set; }
        public string Bairro { get; set; }
        public string Municipio { get; set; }
        public string UF { get; set; }
        public string CEP { get; set; }
        public string Telefone { get; set; }
    }

    [Serializable]
    public struct Produto
    {
        public string Codigo { get; set; }
        public string Descricao { get; set; }
        public string Ncm { get; set; }
        public string Cst { get; set; }
        public string Cfop { get; set; }
        public string UnidadeComercial { get; set; }
        public int Quantidade { get; set; }
        public double ValorUnitario { get; set; }
        public double ValorDesconto { get; set; }
        public double ValorLiquido { get; set; }
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
        public string InscricaoEstadual { get; set; }
        public string Logradouro { get; set; }
        public string Numero { get; set; }
        public string Bairro { get; set; }
        public string Municipio { get; set; }
        public string UF { get; set; }
        public string CEP { get; set; }
        public string Telefone { get; set; }

    }

    [Serializable]
    public struct Transportadora
    {
        public string Nome { get; set; }
        public string ModalidadeFrete { get; set; }
        public string CodigoANT { get; set; }
        public string VeiculoPlaca { get; set; }
        public string VeiculoUF { get; set; }
        public string CpfCnpj { get; set; }
        public string EnderecoCompleto { get; set; }
        public string Municipio { get; set; }
        public string UF { get; set; }
        public string InscricaoEstadual { get; set; }
    }

    [Serializable]
    public struct CalculoImposto
    {
        public double BaseCalculo { get; set; }
        public double BaseCalculoST { get; set; }
        public double ValorDespesasAcessorias { get; set; }
        public double ValorTotalAproximado { get; set; }
        public double ValorTotalCofins { get; set; }
        public double ValorTotalDesconto { get; set; }
        public double ValorTotalDesonerado { get; set; }
        public double ValorTotalFrete { get; set; }
        public double ValorTotalIcms { get; set; }
        public double ValorTotalII { get; set; }
        public double ValorTotalIpi { get; set; }
        public double ValorTotalNFe { get; set; }
        public double ValorTotalPis { get; set; }
        public double ValorTotalProdutos { get; set; }
        public double ValorTotalSeguro { get; set; }
        public double ValorTotalST { get; set; }
    }
}