using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFe.Core
{
    public class IcmsSubstituicaoTributariaRetidoAnteiormente : Icms
    {
        public IcmsSubstituicaoTributariaRetidoAnteiormente(CstEnum cst, OrigemMercadoria origem, decimal aliquota) : base(cst, origem, aliquota)
        {
        }
    }
}
