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
        public IcmsCobradoAnteriormentePorSubstituicaoTributaria(decimal valorEfetivo, decimal aliquotaEfetiva, decimal baseCalculoEfetiva, decimal percentualReducaoBaseCalculoEfetiva, decimal valorFundoCombatePobreza, decimal percentualFundoCombatePobreza, decimal baseCalculoFundoCombatePobreza, decimal aliquotaSuportadaConsumidorFinal, decimal valor, decimal baseCalculo, OrigemMercadoria origem) : base("60", origem)
        {
            ValorEfetivo = valorEfetivo;
            AliquotaEfetiva = aliquotaEfetiva;
            BaseCalculoEfetiva = baseCalculoEfetiva;
            PercentualReducaoBaseCalculoEfetiva = percentualReducaoBaseCalculoEfetiva;
            ValorFundoCombatePobreza = valorFundoCombatePobreza;
            PercentualFundoCombatePobreza = percentualFundoCombatePobreza;
            BaseCalculoFundoCombatePobreza = baseCalculoFundoCombatePobreza;
            AliquotaSuportadaConsumidorFinal = aliquotaSuportadaConsumidorFinal;
            Valor = valor;
            BaseCalculo = baseCalculo;
        }

        public decimal BaseCalculo { get;  }
        public decimal AliquotaSuportadaConsumidorFinal { get;  }
        public decimal Valor { get;  }
        public decimal BaseCalculoFundoCombatePobreza { get;  }
        public decimal PercentualFundoCombatePobreza { get;  }
        public decimal ValorFundoCombatePobreza { get;  }
        public decimal PercentualReducaoBaseCalculoEfetiva { get;  }
        public decimal BaseCalculoEfetiva { get;  }
        public decimal AliquotaEfetiva { get;  }
        public decimal ValorEfetivo { get;  }
    }
}