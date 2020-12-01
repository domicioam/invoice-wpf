using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NFe.Core
{
    public class Icms00 : Icms, HasFundoCombatePobreza
    {
        public Icms00(Icms.CstEnum cst, OrigemMercadoria origem, decimal aliquota) : base(cst, origem, aliquota)
        {
        }

        public NotasFiscais.FundoCombatePobreza FundoCombatePobreza => throw new NotImplementedException();
    }
}