using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NFe.Core
{
    public abstract class IcmsBase : Imposto
    {
        public string Cst { get; }
        public OrigemMercadoria Origem { get; }
    }
}