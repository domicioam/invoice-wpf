using NFe.Core.NotasFiscais.Impostos.Icms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NFe.Core
{
    internal class Icms70 : IcmsDesonerado
    {
        public Icms70(Desoneracao desoneracaoIcms, Icms.CstEnum cst, OrigemMercadoria origem) : base(desoneracaoIcms, cst, origem)
        {
        }
    }
}