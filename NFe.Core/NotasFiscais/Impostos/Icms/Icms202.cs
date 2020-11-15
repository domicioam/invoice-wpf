using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NFe.Core
{
    public class Icms202 : IcmsSn
    {
        public Icms202(CstEnum cst, OrigemMercadoria origem) : base(cst, origem)
        {
        }
    }
}