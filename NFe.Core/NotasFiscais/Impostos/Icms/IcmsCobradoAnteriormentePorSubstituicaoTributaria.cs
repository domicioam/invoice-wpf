using NFe.Core.NotasFiscais;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NFe.Core
{

    public class IcmsCobradoAnteriormentePorSubstituicaoTributaria : Icms
    {
        public IcmsCobradoAnteriormentePorSubstituicaoTributaria(decimal aliquota, decimal baseCalculo, FundoCombatePobreza fcp, OrigemMercadoria origem) : base(CstEnum.CST60, origem)
        {
            Aliquota = aliquota;
            BaseCalculo = baseCalculo;
            _fcp = fcp;
        }


        private FundoCombatePobreza _fcp;
        public override decimal BaseCalculo { get;  }

        public override decimal Valor { get { return BaseCalculo * (Aliquota / 100); }  } // Ver casos onde o cálculo deve ser por dentro
        public decimal BaseCalculoFundoCombatePobreza { get { return _fcp.BaseCalculo; } }
        public decimal AliquotaFCP { get { return _fcp.Aliquota; }  }
        public decimal ValorFundoCombatePobreza { get { return _fcp.Valor; }  }
        public decimal Aliquota { get;  }
    }
}
 