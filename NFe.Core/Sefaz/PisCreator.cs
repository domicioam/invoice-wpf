using NFe.Core.Domain;
using NFe.Core.XmlSchemas.NfeAutorizacao.Envio;
using System;

namespace NFe.Core.Sefaz
{
    internal class PisCreator : IImpostoCreator
    {
        public object Create(Domain.Interface.Imposto impostoItem)
        {
            if (!(impostoItem is Pis pis))
            {
                throw new ArgumentException("Imposto não é pis.");
            }

            var pisDet = new TNFeInfNFeDetImpostoPIS();
            TNFeInfNFeDetImpostoPISPISNT pisDetNt;

            if (pis.Cst == Pis.CstEnum.CST04)
            {
                var pisMonofasico = (PisOperacaoTributavelMonofasica)pis;
                pisDetNt = new TNFeInfNFeDetImpostoPISPISNT()
                {
                    CST = pisMonofasico.Cst
                };
            }
            else if (pis.Cst == Pis.CstEnum.CST07)
            {
                var pisIsento = (PisOperacaoIsentaContribuicao)pis;
                pisDetNt = new TNFeInfNFeDetImpostoPISPISNT()
                {
                    CST = pisIsento.Cst
                };
            }
            else
            {
                throw new NotSupportedException("CST não suportado.");
            }

            pisDet.Item = pisDetNt;
            return pisDet;
        }
    }
}