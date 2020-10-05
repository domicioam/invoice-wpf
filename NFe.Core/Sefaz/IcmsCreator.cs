using NFe.Core.NotasFiscais;
using NFe.Core.Sefaz.Utility;
using NFe.Core.XmlSchemas.NfeAutorizacao.Envio;
using System;

namespace NFe.Core.Sefaz
{
    internal class IcmsCreator : IImpostoCreator
    {
        public object Create(Imposto impostoItem)
        {
            if (!(impostoItem is Icms icms))
            {
                throw new ArgumentException("Imposto não é Icms.");
            }

            var icmsDet = new TNFeInfNFeDetImpostoICMS();

            switch (icms.Cst)
            {
                case Icms.CstEnum.CST60:
                    var icms60 = (IcmsCobradoAnteriormentePorSubstituicaoTributaria)icms;

                    var icmsDet60 = new TNFeInfNFeDetImpostoICMSICMS60
                    {
                        orig = icms60.Origem.ToTorig(),
                        CST = icms60.Cst.ToTNFeInfNFeDetImpostoICMSICMS60CST(),
                        vBCSTRet = icms60.BaseCalculo.ToPositiveDecimalAsStringOrNull(),
                        pST = icms60.Aliquota.ToPositiveDecimalAsStringOrNull(),
                        vICMSSTRet = icms60.Valor.ToPositiveDecimalAsStringOrNull(),
                        pFCPSTRet = icms60.PercentualFundoCombatePobreza.ToPositiveDecimalAsStringOrNull(),
                        vBCFCPSTRet = icms60.BaseCalculoFundoCombatePobreza.ToPositiveDecimalAsStringOrNull(),
                        vFCPSTRet = icms60.ValorFundoCombatePobreza.ToPositiveDecimalAsStringOrNull()
                    };

                    icmsDet.Item = icmsDet60;
                    break;
                case Icms.CstEnum.CST41:
                    var icms41 = (IcmsNaoTributado)icms;

                    var icmsDet41 = new TNFeInfNFeDetImpostoICMSICMS40
                    {
                        orig = icms41.Origem.ToTorig(),
                        CST = icms41.Cst.ToTNFeInfNFeDetImpostoICMSICMS40CST()
                    };

                    if(icms41.DesoneracaoIcms != null)
                    {
                        icmsDet41.vICMSDeson = icms41.DesoneracaoIcms.ValorDesonerado.ToPositiveDecimalAsStringOrNull();
                        icmsDet41.motDesICMS = icms41.DesoneracaoIcms.MotivoDesoneracao.ToTNFeInfNFeDetImpostoICMSICMS40MotDesICMS();
                    }

                    icmsDet.Item = icmsDet41;
                    break;
                default:
                    throw new NotSupportedException($"CST ainda não suportado: {icms.Cst}");
            }

            return icmsDet;
        }
    }
}