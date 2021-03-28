using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFe.Core.NotasFiscais
{
    public class FundoCombatePobreza
    {
        public FundoCombatePobreza(decimal aliquota, decimal baseCalculo)
        {
            Aliquota = aliquota;
            BaseCalculo = baseCalculo;
        }

        public decimal Aliquota { get; set; }
        public decimal BaseCalculo { get; set; }
        public decimal Valor { get { return BaseCalculo * (Aliquota / 100); } }
    }
}
