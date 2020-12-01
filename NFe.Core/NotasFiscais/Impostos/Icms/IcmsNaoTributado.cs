using NFe.Core.NotasFiscais.Impostos.Icms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NFe.Core
{
    /// <summary>
    /// Icms não tributado talvez seja uma superclasse para os icms 40, 41 e 50 porque não possui campos que a classe Icms possui (base cálculo, valor)
    /// </summary>
    internal class IcmsNaoTributado : IcmsDesonerado
    {

        public IcmsNaoTributado(Desoneracao desoneracaoIcms, OrigemMercadoria origem) : base(desoneracaoIcms, CstEnum.CST41, origem)
        {
        }
    }
}