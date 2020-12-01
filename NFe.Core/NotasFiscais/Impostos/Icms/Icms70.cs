using NFe.Core.NotasFiscais.Impostos.Icms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NFe.Core
{
    internal class Icms70 : IcmsSubstituicaoTributaria, IcmsDesonerado
    {
        public Icms70(Desoneracao desoneracaoIcms, Icms.CstEnum cst, OrigemMercadoria origem, NotasFiscais.FundoCombatePobreza fundoCombatePobreza) : base(cst, origem, fundoCombatePobreza)
        {
            Desoneracao = desoneracaoIcms;
        }

        public Desoneracao Desoneracao { get; }
    }
}