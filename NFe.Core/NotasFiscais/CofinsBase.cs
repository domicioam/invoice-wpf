using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NFe.Core.NotasFiscais;

namespace NFe.Core
{
    public abstract class CofinsBase : Imposto
    {
        protected CofinsBase(CstEnum cst)
        {
            Cst = cst;
        }

        public CstEnum Cst { get; }

        public enum CstEnum
        {
            CST01 = 01,
            CST02 = 02
        }
    }
}