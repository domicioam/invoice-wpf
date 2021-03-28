using NFe.Core.NotaFiscal;
using NFe.Core.NotasFiscais;
using NFe.Core.NotasFiscais.Impostos.Icms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NFe.Core
{
    /// <summary>
    /// Esse Icms possui campos de Icms normal e de Icms por ST
    /// </summary>
    internal class Icms90 : Icms, IcmsDesonerado, HasFundoCombatePobreza, HasSubstituicaoTributaria
    {
        public Icms90(Desoneracao desoneracaoIcms, CstEnum cst, OrigemMercadoria origem, NotasFiscais.FundoCombatePobreza fundoCombatePobreza, decimal aliquota) : base(cst, origem, aliquota)
        {
            Desoneracao = desoneracaoIcms;
        }

        public Desoneracao Desoneracao { get; }
        public SubstituicaoTributaria SubstituicaoTributaria { get; }

        public FundoCombatePobreza FundoCombatePobreza => throw new NotImplementedException();
    }
}