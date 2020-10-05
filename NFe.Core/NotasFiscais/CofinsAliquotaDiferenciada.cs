using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NFe.Core
{
    public class CofinsAliquotaDiferenciada : CofinsAliq
    {
        public CofinsAliquotaDiferenciada(decimal baseCalculo, decimal aliquota) : base(CstEnum.CST02, baseCalculo, aliquota)
        {
        }
    }
}