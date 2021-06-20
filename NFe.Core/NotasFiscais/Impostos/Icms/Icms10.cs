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
    internal class Icms10 : Icms, HasFundoCombatePobreza, HasSubstituicaoTributaria
    {
        public Icms10(decimal aliquota, decimal baseCalculo, CstEnum cst, OrigemMercadoria origem, NotasFiscais.FundoCombatePobreza fundoCombatePobreza, SubstituicaoTributaria substituicaoTributaria) : base(cst, origem, aliquota)
        {
            BaseCalculo = baseCalculo;
            FundoCombatePobreza = fundoCombatePobreza;
            SubstituicaoTributaria = substituicaoTributaria;
        }

        public override decimal BaseCalculo { get; }
        public override decimal Valor { get { return BaseCalculo * (Aliquota / 100); } } // Ver casos onde o cálculo deve ser por dentro
        public SubstituicaoTributaria SubstituicaoTributaria { get; }

        public NotasFiscais.FundoCombatePobreza FundoCombatePobreza { get; }
    }
}