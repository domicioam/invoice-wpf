using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NFe.Core
{
    public class IcmsPartilha : Icms
    {
        public IcmsPartilha(string cst, OrigemMercadoria origem) : base(cst, origem)
        {
        }
    }
}