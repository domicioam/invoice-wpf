using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFe.Core.NotasFiscais.Impostos.Icms
{
    internal class SubstituicaoTributaria : HasFundoCombatePobreza
    {
        public SubstituicaoTributaria(FundoCombatePobreza fundoCombatePobreza, decimal baseCalculo, decimal aliquota)
        {
            FundoCombatePobreza = fundoCombatePobreza;
            BaseCalculo = baseCalculo;
            Aliquota = aliquota;
        }

        public FundoCombatePobreza FundoCombatePobreza { get; }
        public decimal BaseCalculo { get; }
        public decimal Valor { get; }
        public decimal Aliquota { get; }
    }
}
