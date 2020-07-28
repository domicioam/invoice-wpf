using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NFe.Core
{
    public class Icms201 : IcmsSn
    {
        public Icms201(string cst, OrigemMercadoria origem) : base(cst, origem)
        {
        }
    }
}