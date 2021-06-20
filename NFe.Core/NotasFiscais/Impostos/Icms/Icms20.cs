using NFe.Core.NotasFiscais.Impostos.Icms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NFe.Core
{
    internal class Icms20 : Icms, IcmsDesonerado, HasFundoCombatePobreza
    {
        public Icms20(Desoneracao desoneracaoIcms, CstEnum cst, OrigemMercadoria origem, decimal aliquota) : base(cst, origem, aliquota)
        {
            Desoneracao = desoneracaoIcms;
        }

        public Desoneracao Desoneracao { get; }

        public NotasFiscais.FundoCombatePobreza FundoCombatePobreza => throw new NotImplementedException();
    }
}