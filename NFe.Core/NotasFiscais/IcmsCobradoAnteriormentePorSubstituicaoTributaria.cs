using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NFe.Core
{
    public enum OrigemMercadoria
    {
        Nacional,
        EstrangeiraImportacaoDireta,
        EstrangeiraMercadoInterno
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