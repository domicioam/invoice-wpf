using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFe.Core.NotasFiscais.Impostos.Icms
{
    class IcmsDesonerado : Core.Icms
    {
        public IcmsDesonerado(Desoneracao desoneracaoIcms, CstEnum cst, OrigemMercadoria origem) : base(cst, origem)
        {
            Desoneracao = desoneracaoIcms;
        }

        public Desoneracao Desoneracao { get; }
    }
}
