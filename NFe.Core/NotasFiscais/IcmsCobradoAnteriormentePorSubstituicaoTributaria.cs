using NFe.Core.Extensions;
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
            if(origemMercadoria == Nacional)
            {
                return Torig.Item0;
            } else if(origemMercadoria == EstrangeiraImportacaoDireta)
            {
                return Torig.Item1;
            } else if (origemMercadoria == EstrangeiraMercadoInterno)
            {
                return Torig.Item2;
            } else
            {
                throw new InvalidOperationException($"Origem de mercadoria não suportada: {origemMercadoria}.");
            }
        }
    }

    public class IcmsCobradoAnteriormentePorSubstituicaoTributaria : Icms
    {
        public IcmsCobradoAnteriormentePorSubstituicaoTributaria(decimal valor, decimal aliquota, decimal baseCalculo, decimal valorFundoCombatePobreza, decimal percentualFundoCombatePobreza, decimal baseCalculoFundoCombatePobreza, OrigemMercadoria origem) : base(CstEnum.CST60, origem)
        {
            Aliquota = aliquota;
            ValorFundoCombatePobreza = valorFundoCombatePobreza;
            PercentualFundoCombatePobreza = percentualFundoCombatePobreza;
            BaseCalculoFundoCombatePobreza = baseCalculoFundoCombatePobreza;
            Valor = valor;
            BaseCalculo = baseCalculo;
        }

        public decimal BaseCalculo { get;  }
        public decimal Valor { get;  } // É calculado?
        public decimal BaseCalculoFundoCombatePobreza { get;  }
        public decimal PercentualFundoCombatePobreza { get;  }
        public decimal ValorFundoCombatePobreza { get;  }
        public decimal Aliquota { get;  }
    }
}
 