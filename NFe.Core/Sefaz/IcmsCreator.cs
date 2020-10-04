using NFe.Core.NotasFiscais;
using NFe.Core.Sefaz.Utility;
using NFe.Core.XmlSchemas.NfeAutorizacao.Envio;
using System;

namespace NFe.Core.Sefaz
{
    internal class IcmsCreator : IImpostoCreator
    {
        public IcmsCreator()
        {
        }

        /*
        private static TNFeInfNFeDetImpostoICMS GetImpostoIcms(Produto produto)
        {
            var icms = new TNFeInfNFeDetImpostoICMS();

            switch (produto.Impostos.GetIcmsCst())
            {
                case TabelaIcmsCst.IcmsCobradoAnteriormentePorST:
                    var icms60 = new TNFeInfNFeDetImpostoICMSICMS60
                    {
                        orig = Torig.Item0,
                        CST = TNFeInfNFeDetImpostoICMSICMS60CST.Item60
                    };
                    icms.Item = icms60;
                    break;

                case TabelaIcmsCst.NaoTributada:
                    var icms41 = new TNFeInfNFeDetImpostoICMSICMS40
                    {
                        orig = Torig.Item0,
                        CST = TNFeInfNFeDetImpostoICMSICMS40CST.Item41
                    };
                    icms.Item = icms41;
                    break;

                default:
                    throw new ArgumentException();
            }

            return icms;
        }
         */

        public object Create(Imposto impostoItem)
        {
            if (!(impostoItem is Icms icms))
            {
                throw new ArgumentException("Imposto não é Icms.");
            }

            var icmsDet = new TNFeInfNFeDetImpostoICMS();

            switch (icms.Cst)
            {
                case Icms.CstEnum.CST40:
                    break;
                case Icms.CstEnum.CST60:
                    var icms60 = (IcmsCobradoAnteriormentePorSubstituicaoTributaria)icms;

                    var icmsDet60 = new TNFeInfNFeDetImpostoICMSICMS60();
                    icmsDet60.orig = icms60.Origem.ToTorig();
                    icmsDet60.CST = icms60.Cst.ToTNFeInfNFeDetImpostoICMSICMS60CST();
                    icmsDet60.vBCSTRet = icms60.BaseCalculo.ToPositiveDecimalAsStringOrNull();
                    icmsDet60.pST = icms60.Aliquota.ToPositiveDecimalAsStringOrNull();
                    icmsDet60.vICMSSTRet = icms60.Valor.ToPositiveDecimalAsStringOrNull();
                    icmsDet60.pFCPSTRet = icms60.PercentualFundoCombatePobreza.ToPositiveDecimalAsStringOrNull();
                    icmsDet60.vBCFCPSTRet = icms60.BaseCalculoFundoCombatePobreza.ToPositiveDecimalAsStringOrNull();
                    icmsDet60.vFCPSTRet = icms60.ValorFundoCombatePobreza.ToPositiveDecimalAsStringOrNull();

                    icmsDet.Item = icmsDet60;
                    break;
                case Icms.CstEnum.CST41:
                    var icms41 = new TNFeInfNFeDetImpostoICMSICMS40
                    {
                        orig = Torig.Item0,
                        CST = TNFeInfNFeDetImpostoICMSICMS40CST.Item41
                    };
                    icmsDet.Item = icms41;
                    break;
                default:
                    throw new NotSupportedException("CST ainda não suportado.");
            }


            throw new System.NotImplementedException();
        }
    }
}