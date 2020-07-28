using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NFe.Core
{
    public abstract class CofinsBase : Imposto
    {
        protected CofinsBase(string cst)
        {
            Cst = cst;
        }

        public string Cst { get; }
    }
}