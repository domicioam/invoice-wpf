using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NFe.Core
{
    public class CofinsAliquotaDiferenciada : CofinsAliq
    {
        public CofinsAliquotaDiferenciada(decimal baseCalculo, decimal aliquota) : base("02", baseCalculo, aliquota)
        {
        }
    }
}