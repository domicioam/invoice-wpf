using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NFe.Core
{
    public class Icms500 : IcmsSn
    {
        public Icms500(CstEnum cst, OrigemMercadoria origem) : base(cst, origem)
        {
        }
    }
}