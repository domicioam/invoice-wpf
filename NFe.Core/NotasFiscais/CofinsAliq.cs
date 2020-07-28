using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NFe.Core
{
    public abstract class CofinsAliq : CofinsBase
    {
        public decimal BaseCalculo { get; }
        public decimal Percentagem { get; }
        public decimal Valor { get; }

        protected CofinsAliq(string cst, decimal baseCalculo, decimal percentagem, decimal valor) : base(cst)
        {
            BaseCalculo = baseCalculo;
            Percentagem = percentagem;
            Valor = valor;
        }
    }
}