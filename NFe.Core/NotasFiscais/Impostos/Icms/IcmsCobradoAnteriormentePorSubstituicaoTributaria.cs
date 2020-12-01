using NFe.Core.NotasFiscais;
using NFe.Core.NotasFiscais.Impostos.Icms;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NFe.Core
{

    public class IcmsCobradoAnteriormentePorSubstituicaoTributaria : IcmsSubstituicaoTributariaRetidoAnteiormente, HasFundoCombatePobreza
    {
        public IcmsCobradoAnteriormentePorSubstituicaoTributaria(decimal aliquota, decimal baseCalculo, NotasFiscais.FundoCombatePobreza fcp, OrigemMercadoria origem) : base(CstEnum.CST60, origem, aliquota)
        {
            BaseCalculo = baseCalculo;
            FundoCombatePobreza = fcp;
        }

        public NotasFiscais.FundoCombatePobreza FundoCombatePobreza { get; }
        public override decimal BaseCalculo { get;  }
        public override decimal Valor { get { return BaseCalculo * (Aliquota / 100); }  } // Ver casos onde o cálculo deve ser por dentro
    }
}
 