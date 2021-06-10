using System;

namespace DgSystem.NFe.Reports.Nfe
{
    [Serializable]
    public struct CalculoImposto
    {
        public CalculoImposto(double baseCalculo, double baseCalculoST, double valorDespesasAcessorias,
            double valorTotalAproximado, double valorTotalCofins, double valorTotalDesconto, double valorTotalDesonerado,
            double valorTotalFrete, double valorTotalIcms, double valorTotalII, double valorTotalIpi, double valorTotalNFe,
            double valorTotalPis, double valorTotalProdutos, double valorTotalSeguro, double valorTotalST,
            double totalOutros, double valorTotalAproximadoTributos)
        {
            BaseCalculo = baseCalculo;
            BaseCalculoST = baseCalculoST;
            ValorDespesasAcessorias = valorDespesasAcessorias;
            ValorTotalAproximado = valorTotalAproximado;
            ValorTotalCofins = valorTotalCofins;
            ValorTotalDesconto = valorTotalDesconto;
            ValorTotalDesonerado = valorTotalDesonerado;
            ValorTotalFrete = valorTotalFrete;
            ValorTotalIcms = valorTotalIcms;
            ValorTotalII = valorTotalII;
            ValorTotalIpi = valorTotalIpi;
            ValorTotalNFe = valorTotalNFe;
            ValorTotalPis = valorTotalPis;
            ValorTotalProdutos = valorTotalProdutos;
            ValorTotalSeguro = valorTotalSeguro;
            ValorTotalST = valorTotalST;
            TotalOutros = totalOutros;
            ValorTotalAproximadoTributos = valorTotalAproximadoTributos;
        }

        public double BaseCalculo { get; }
        public double BaseCalculoST { get; }
        public double ValorDespesasAcessorias { get; }
        public double ValorTotalAproximado { get; }
        public double ValorTotalCofins { get; }
        public double ValorTotalDesconto { get; }
        public double ValorTotalDesonerado { get; }
        public double ValorTotalFrete { get; }
        public double ValorTotalIcms { get; }
        public double ValorTotalII { get; }
        public double ValorTotalIpi { get; }
        public double ValorTotalNFe { get; }
        public double ValorTotalPis { get; }
        public double ValorTotalProdutos { get; }
        public double ValorTotalSeguro { get; }
        public double ValorTotalST { get; }
        public double TotalOutros { get; }
        public double ValorTotalAproximadoTributos { get; }
    }
}