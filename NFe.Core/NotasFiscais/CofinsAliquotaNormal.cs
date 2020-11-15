using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFe.Core.NotasFiscais
{
    class CofinsAliquotaNormal : CofinsAliq
    {
        public CofinsAliquotaNormal(decimal baseCalculo, decimal aliquota) : base(CstEnum.CST01, baseCalculo, aliquota)
        {
        }
    }
}
