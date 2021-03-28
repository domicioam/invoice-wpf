using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NFe.Core
{
    public class IcmsSn90 : Icms
    {
        public IcmsSn90(CstEnum cst, OrigemMercadoria origem, decimal aliquota) : base(cst, origem, aliquota)
        {
        }
    }
}