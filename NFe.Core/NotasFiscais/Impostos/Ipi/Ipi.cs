using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NFe.Core.Domain;

namespace NFe.Core
{
    public abstract class Ipi : Imposto
    {
        public decimal Valor { get; set; }
    }
}