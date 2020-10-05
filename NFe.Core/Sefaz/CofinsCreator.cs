using NFe.Core.NotasFiscais;
using NFe.Core.Sefaz.Utility;
using NFe.Core.XmlSchemas.NfeAutorizacao.Envio;
using System;

namespace NFe.Core.Sefaz
{
    internal class CofinsCreator : IImpostoCreator
    {
        public object Create(Imposto impostoItem)
        {
            if (!(impostoItem is CofinsAliq cofinsAliq))
            {
                throw new ArgumentException("Imposto não é confins aliquota.");
            }

            var cofinsDet = new TNFeInfNFeDetImpostoCOFINS();
            TNFeInfNFeDetImpostoCOFINSCOFINSAliq cofinsDetAliq;

            if (cofinsAliq.Cst == CofinsBase.CstEnum.CST01)
            {
                var cofinsCumulativo = (CofinsCumulativoNaoCumulativo)cofinsAliq;
                cofinsDetAliq = new TNFeInfNFeDetImpostoCOFINSCOFINSAliq()
                {
                    CST = cofinsCumulativo.Cst,
                    pCOFINS = cofinsCumulativo.Aliquota.ToPositiveDecimalAsStringOrNull(),
                    vBC = cofinsCumulativo.BaseCalculo.ToPositiveDecimalAsStringOrNull(),
                    vCOFINS = cofinsCumulativo.Valor.ToPositiveDecimalAsStringOrNull()
                };
            }
            else if (cofinsAliq.Cst == CofinsBase.CstEnum.CST02)
            {
                var cofinsAliquotaDiferenciada = (CofinsAliquotaDiferenciada)cofinsAliq;
                cofinsDetAliq = new TNFeInfNFeDetImpostoCOFINSCOFINSAliq()
                {
                    CST = cofinsAliquotaDiferenciada.Cst,
                    pCOFINS = cofinsAliquotaDiferenciada.Aliquota.ToPositiveDecimalAsStringOrNull(),
                    vBC = cofinsAliquotaDiferenciada.BaseCalculo.ToPositiveDecimalAsStringOrNull(),
                    vCOFINS = cofinsAliquotaDiferenciada.Valor.ToPositiveDecimalAsStringOrNull()
                };
            }
            else
            {
                throw new NotSupportedException("CST não suportado.");
            }

            cofinsDet.Item = cofinsDetAliq;
            return cofinsDet;
        }
    }
}