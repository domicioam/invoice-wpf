using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NFe.Core
{
    internal class Icms10 : IcmsSubstituicaoTributaria
    {
        public Icms10(decimal aliquota, decimal baseCalculo, CstEnum cst, OrigemMercadoria origem, NotasFiscais.FundoCombatePobreza fundoCombatePobreza) : base(cst, origem, fundoCombatePobreza)
        {
            Aliquota = aliquota;
            BaseCalculo = baseCalculo;
        }

        public override decimal BaseCalculo { get; }
        public override decimal Valor { get { return BaseCalculo * (Aliquota / 100); } } // Ver casos onde o cálculo deve ser por dentro
        public decimal Aliquota { get; }
    }
}