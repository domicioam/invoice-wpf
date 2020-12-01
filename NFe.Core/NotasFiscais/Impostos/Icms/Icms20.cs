using NFe.Core.NotasFiscais.Impostos.Icms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NFe.Core
{
    internal class Icms20 : Icms, IcmsDesonerado
    {
        public Icms20(Desoneracao desoneracaoIcms, CstEnum cst, OrigemMercadoria origem) : base(cst, origem)
        {
            Desoneracao = desoneracaoIcms;
        }

        public Desoneracao Desoneracao { get; }
    }
}