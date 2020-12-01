using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NFe.Core
{
    public class Icms900 : IcmsSubstituicaoTributaria
    {
        public Icms900(CstEnum cst, OrigemMercadoria origem, NotasFiscais.FundoCombatePobreza fundoCombatePobreza) : base(cst, origem, fundoCombatePobreza)
        {
        }
    }
}