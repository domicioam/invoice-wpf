using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NFe.Core
{
    public class IcmsSubstituicaoTributaria : Icms, FundoCombatePobreza
    {
        public IcmsSubstituicaoTributaria(CstEnum cst, OrigemMercadoria origem, NotasFiscais.FundoCombatePobreza fundoCombatePobreza) : base(cst, origem)
        {
            FundoCombatePobreza = fundoCombatePobreza;
        }

        public NotasFiscais.FundoCombatePobreza FundoCombatePobreza { get; }
    }
}