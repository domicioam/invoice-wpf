using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NFe.Core
{
    public class IcmsNaoTributado : IcmsBase
    {
        internal DesoneracaoIcms DesoneracaoIcms { get; }

        public IcmsNaoTributado(DesoneracaoIcms desoneracaoIcms)
        {
            Cst = "41";
            DesoneracaoIcms = desoneracaoIcms;
        }
    }
}