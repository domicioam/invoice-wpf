using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NFe.Core
{
    internal class IcmsNaoTributado : Icms
    {
        internal DesoneracaoIcms DesoneracaoIcms { get; }

        public IcmsNaoTributado(DesoneracaoIcms desoneracaoIcms, OrigemMercadoria origem) : base(CstEnum.CST41, origem)
        {
            DesoneracaoIcms = desoneracaoIcms;
        }
    }
}