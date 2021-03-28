using NFe.Core.NotaFiscal;
using NFe.Core.Sefaz.Utility;
using NFe.Core.XmlSchemas.NfeAutorizacao.Envio;
using System;

namespace NFe.Core.Sefaz
{
    internal class CofinsCreator : IImpostoCreator
    {
        public object Create(NotaFiscal.Interface.Imposto impostoItem)
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
                    pCOFINS = cofinsCumulativo.Aliquota.ToString(),
                    vBC = cofinsCumulativo.BaseCalculo.ToString(),
                    vCOFINS = cofinsCumulativo.Valor.ToString()
                };
            }
            else if (cofinsAliq.Cst == CofinsBase.CstEnum.CST02)
            {
                var cofinsAliquotaDiferenciada = (CofinsAliquotaDiferenciada)cofinsAliq;
                cofinsDetAliq = new TNFeInfNFeDetImpostoCOFINSCOFINSAliq()
                {
                    CST = cofinsAliquotaDiferenciada.Cst,
                    pCOFINS = cofinsAliquotaDiferenciada.Aliquota.ToString(),
                    vBC = cofinsAliquotaDiferenciada.BaseCalculo.ToString(),
                    vCOFINS = cofinsAliquotaDiferenciada.Valor.ToString()
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