using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NFe.Core
{
    public class IcmsSubstituicaoTributaria : Icms
    {
        public IcmsSubstituicaoTributaria(CstEnum cst, OrigemMercadoria origem) : base(cst, origem)
        {
        }
    }
}