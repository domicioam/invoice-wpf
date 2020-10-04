using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NFe.Core.NotasFiscais;

namespace NFe.Core
{
    public abstract class Icms : Imposto
    {
        protected Icms(CstEnum cst, OrigemMercadoria origem)
        {
            Cst = cst;
            Origem = origem;
        }

        public CstEnum Cst { get; }
        public OrigemMercadoria Origem { get; }

        public enum CstEnum
        {
            CST60 = 60,
            CST41 = 41,
            CST40 = 40
        }
    }
}