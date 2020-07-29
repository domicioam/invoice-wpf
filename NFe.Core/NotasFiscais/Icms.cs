using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NFe.Core.NotasFiscais;

namespace NFe.Core
{
    public abstract class IcmsBase : Imposto
    {
        protected IcmsBase(string cst, OrigemMercadoria origem)
        {
            Cst = cst;
            Origem = origem;
        }

        public string Cst { get; }
        public OrigemMercadoria Origem { get; }
        

    }
}