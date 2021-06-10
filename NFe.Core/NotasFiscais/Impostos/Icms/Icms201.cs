using NFe.Core.NotasFiscais.Impostos.Icms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NFe.Core
{
    internal class Icms201 : Icms, HasSubstituicaoTributaria
    {
        public Icms201(CstEnum cst, OrigemMercadoria origem, NotasFiscais.FundoCombatePobreza fundoCombatePobreza, decimal aliquota) : base(cst, origem, aliquota)
        {
        }

        public SubstituicaoTributaria SubstituicaoTributaria { get; }
    }
}