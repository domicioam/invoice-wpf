using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NFe.Core.NotasFiscais;

namespace NFe.Core
{
    public abstract class PisBase : Imposto
    {
        protected PisBase(string cst)
        {
            Cst = cst;
        }

        public string Cst { get; }
    }
}