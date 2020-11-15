using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NFe.Core
{
    public abstract class IcmsSn : Icms
    {
        protected IcmsSn(CstEnum cst, OrigemMercadoria origem) : base(cst, origem)
        {
        }
    }
}