using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NFe.Core
{
    /// <summary>
    /// Equivalente do ICMS 60 no Simples Nacional
    /// </summary>
    public class Icms500 : IcmsSn, IFundoCombatePobreza
    {
        public Icms500(CstEnum cst, OrigemMercadoria origem) : base(cst, origem)
        {
        }

        public decimal AliquotaFCP => throw new NotImplementedException();

        public decimal ValorFundoCombatePobreza => throw new NotImplementedException();
    }
}