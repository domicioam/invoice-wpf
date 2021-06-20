using NFe.Core.NotasFiscais.Impostos.Icms;
using System.Collections.Generic;
using System.Linq;

namespace NFe.Core.Domain
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

        public static IcmsTotal CalculateIcmsTotal(IReadOnlyCollection<Produto> produtos)
        {
            var impostos = produtos.SelectMany(p => p.Impostos);
            var impostosIcms = impostos.Where(i => i is Icms);
            var icmsDesonerados = impostos.Where(i => i is IcmsDesonerado);
            var icmsRetidoAnteriormente = impostos.Where(i => i is IcmsSubstituicaoTributariaRetidoAnteiormente);
            var icmsSubstituicaoTributaria = impostos.Where(i => i is HasSubstituicaoTributaria);
            var impostosII = impostos.Where(i => i is II);
            var impostosIpi = impostos.Where(i => i is Ipi);
            var impostosPis = impostos.Where(i => i is Pis);
            var impostosCofins = impostos.Where(i => i is CofinsBase);

            var valorTotalST = icmsSubstituicaoTributaria.Sum(i =>
                (double)((HasSubstituicaoTributaria)i).SubstituicaoTributaria.Valor);
            var valorTotalFCPST = icmsSubstituicaoTributaria.Sum(i =>
                (double)((HasSubstituicaoTributaria)i).SubstituicaoTributaria.FundoCombatePobreza.Valor);
            var valorTotalFrete = produtos.Sum(p => p.Frete);
            var valorTotalSeguro = produtos.Sum(p => p.Seguro);
            var valorTotalOutros = produtos.Sum(p => p.Outros);
            var valorTotalII = impostosII.Sum(i => (double)((II)i).Valor);
            var valorTotalIPI = impostosIpi.Sum(i => (double)((Ipi)i).Valor);
            var valorTotalDesconto = produtos.Sum(p => p.Desconto);
            var valorTotalIcmsDesonerado = icmsDesonerados.Sum(i =>
            {
                var icmsDesonerado = (IcmsDesonerado)i;
                if (icmsDesonerado.Desoneracao != null)
                    return (double)icmsDesonerado.Desoneracao.ValorDesonerado;

                return 0;
            });
            var valorTotalProdutos = produtos.Sum(p => p.ValorTotal);
            var valorTotalBaseCalculoIcms = impostosIcms.Sum(i => (double)((Icms)i).BaseCalculo);
            var valorTotalIcms = impostosIcms.Sum(i => (double)((Icms)i).Valor);
            var valorFCPRetidoAnteriormentePorST =
                icmsRetidoAnteriormente.Sum(i => (double)((HasFundoCombatePobreza)i).FundoCombatePobreza.Valor);
            var valorTotalBaseCalculoPorST = icmsSubstituicaoTributaria.Sum(i =>
                (double)((HasSubstituicaoTributaria)i).SubstituicaoTributaria.BaseCalculo);
            var valorTotalFCP = impostosIcms.Where(i => i is HasFundoCombatePobreza)
                .Sum(i => (double)((HasFundoCombatePobreza)i).FundoCombatePobreza.Valor);
            var valorTotalPis = impostosPis.Sum(i => (double)((Pis)i).Valor);
            var valorTotalCofins = impostosCofins.Sum(i => (double)((CofinsBase)i).Valor);
            var totalNFe = valorTotalProdutos
                           + valorTotalST
                           + valorTotalFCPST
                           + valorTotalFrete
                           + valorTotalSeguro
                           + valorTotalOutros
                           + valorTotalII
                           + valorTotalIPI
                           - valorTotalDesconto
                           - valorTotalIcmsDesonerado;

            var icmsTotal = new IcmsTotal
            {
                BaseCalculo = valorTotalBaseCalculoIcms,
                BaseCalculoST = valorTotalBaseCalculoPorST,
                TotalFundoCombatePobreza = valorTotalFCP,
                TotalFundoCombatePobrezaSubstituicaoTributaria = valorTotalFCPST,
                TotalFundoCombatePobrezaSubstituicaoTributariaRetidoAnteriormente = valorFCPRetidoAnteriormentePorST,
                ValorTotalFrete = valorTotalFrete,
                ValorTotalDesconto = valorTotalDesconto,
                ValorTotalDesonerado = valorTotalIcmsDesonerado,
                ValorTotalCofins = valorTotalCofins,
                ValorTotalNFe = totalNFe,
                ValorTotalIcms = valorTotalIcms,
                ValorTotalSeguro = valorTotalSeguro,
                ValorTotalPis = valorTotalPis,
                ValorTotalProdutos = valorTotalProdutos,
                ValorTotalST = valorTotalST,
                ValorTotalII = valorTotalII,
                ValorTotalIpi = valorTotalIPI,
                TotalOutros = valorTotalOutros
            };

            return icmsTotal;
        }
    }
}