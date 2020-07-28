using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NFe.Core
{
    public class CofinsCumulativoNaoCumulativo : CofinsAliq
    {
        public CofinsCumulativoNaoCumulativo(decimal baseCalculo, decimal percentagem, decimal valor) : base("01", baseCalculo, percentagem, valor)
        {
        }
    }
}