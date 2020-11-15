using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NFe.Core
{
    public abstract class CofinsAliq : CofinsBase
    {
        public decimal BaseCalculo { get; }
        public decimal Aliquota { get; }
        public decimal Valor { get { return BaseCalculo * (Aliquota / 100); } } // Ver casos onde o cálculo deve ser por dentro

        protected CofinsAliq(CstEnum cst, decimal baseCalculo, decimal aliquota) : base(cst)
        {
            BaseCalculo = baseCalculo;
            Aliquota = aliquota;
        }
    }
}