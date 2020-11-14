using NFe.Core.Extensions;
using NFe.Core.NotasFiscais;
using NFe.Core.XmlSchemas.NfeAutorizacao.Envio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NFe.Core
{
    public class OrigemMercadoria : Enumeration
    {
        public static readonly OrigemMercadoria Nacional = new OrigemMercadoria(0, "Nacional");
        public static readonly OrigemMercadoria EstrangeiraImportacaoDireta = new OrigemMercadoria(1, "EstrangeiraImportacaoDireta");
        public static readonly OrigemMercadoria EstrangeiraMercadoInterno = new OrigemMercadoria(2, "EstrangeiraMercadoInterno");

        public OrigemMercadoria(int id, string name) : base(id, name)
        {
        }

        public static implicit operator Torig(OrigemMercadoria origemMercadoria)
        {
            if (origemMercadoria == Nacional)
            {
                return Torig.Item0;
            }
            else if (origemMercadoria == EstrangeiraImportacaoDireta)
            {
                return Torig.Item1;
            }
            else if (origemMercadoria == EstrangeiraMercadoInterno)
            {
                return Torig.Item2;
            }
            else
            {
                throw new InvalidOperationException($"Origem de mercadoria não suportada: {origemMercadoria}.");
            }
        }
    }

    internal class IcmsCobradoAnteriormentePorSubstituicaoTributaria : Icms
    {
        public IcmsCobradoAnteriormentePorSubstituicaoTributaria(decimal valorProduto, BaseCalculo baseCalculo, decimal aliquota, decimal percentualFundoCombatePobreza, BaseCalculoFundoCombatePobreza baseCalculoFundoCombatePobreza, OrigemMercadoria origem) : base(CstEnum.CST60, origem)
        {
            Aliquota = aliquota;
            PercentualFundoCombatePobreza = percentualFundoCombatePobreza;
            BaseCalculoFundoCombatePobreza = baseCalculoFundoCombatePobreza;
            BaseCalculo = baseCalculo;
        }

        public BaseCalculo BaseCalculo { get; } // valor sobre o qual o imposto é calculado
        public decimal Valor { get 
            { 
                return BaseCalculo.Valor * (Aliquota / 100);
            } 
        }
        public BaseCalculoFundoCombatePobreza BaseCalculoFundoCombatePobreza { get; }
        public decimal PercentualFundoCombatePobreza { get; }
        public decimal ValorFundoCombatePobreza { get { return BaseCalculoFundoCombatePobreza.Valor * (PercentualFundoCombatePobreza / 100); } }
        public decimal Aliquota { get; }
    }
}
