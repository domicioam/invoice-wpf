namespace NFe.Core.NotaFiscal
{
    public class IcmsTotal
    {
        public IcmsTotal(double baseCalculo, double valorTotalIcms, double valorTotalDesonerado, double baseCalculoST, double valorTotalST, double valorTotalProdutos, double valorTotalFrete, double valorTotalSeguro, double valorTotalDesconto, double valorTotalII, double valorTotalIpi, double valorTotalPis, double valorTotalCofins, double valorDespesasAcessorias, double valorTotalNFe, double valorTotalAproximadoTributos)
        {
            BaseCalculo = baseCalculo;
            ValorTotalIcms = valorTotalIcms;
            ValorTotalDesonerado = valorTotalDesonerado;
            BaseCalculoST = baseCalculoST;
            ValorTotalST = valorTotalST;
            ValorTotalProdutos = valorTotalProdutos;
            ValorTotalFrete = valorTotalFrete;
            ValorTotalSeguro = valorTotalSeguro;
            ValorTotalDesconto = valorTotalDesconto;
            ValorTotalII = valorTotalII;
            ValorTotalIpi = valorTotalIpi;
            ValorTotalPis = valorTotalPis;
            ValorTotalCofins = valorTotalCofins;
            TotalOutros = valorDespesasAcessorias;
            ValorTotalNFe = valorTotalNFe;
            ValorTotalAproximadoTributos = valorTotalAproximadoTributos;
        }

        public IcmsTotal()
        {
        }

        public double BaseCalculo { get; set; }
        public double ValorTotalIcms { get; set; }
        public double ValorTotalDesonerado { get; set; }
        public double BaseCalculoST { get; set; }
        public double ValorTotalST { get; set; }
        public double ValorTotalProdutos { get; set; }
        public double ValorTotalFrete { get; set; }
        public double ValorTotalSeguro { get; set; }
        public double ValorTotalDesconto { get; set; }
        public double ValorTotalII { get; set; }
        public double ValorTotalIpi { get; set; }
        public double ValorTotalPis { get; set; }
        public double ValorTotalCofins { get; set; }
        public double TotalOutros { get; set; }
        public double ValorTotalNFe { get; set; }
        public double ValorTotalAproximadoTributos { get; set; }
        public double TotalFundoCombatePobreza { get; internal set; }
        public double TotalFundoCombatePobrezaSubstituicaoTributaria { get; set; }
        public double TotalFundoCombatePobrezaSubstituicaoTributariaRetidoAnteriormente { get; internal set; }
        public double TotalIpiDevolvido { get; internal set; }
    }
}