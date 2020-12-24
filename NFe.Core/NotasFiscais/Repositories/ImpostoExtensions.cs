using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DgSystems.NFe.Extensions;
using NFe.Core.Cadastro.Imposto;
using NFe.Core.Entitities.Enums;
using NFe.Core.Utils.Xml;
using NFe.Core.XmlSchemas.NfeAutorizacao.Retorno.NfeProc;

namespace NFe.Core.NotasFiscais.Repositories
{
    internal static class ImpostoExtensions
    {
        public static Entities.Imposto ToImposto(this TNFeInfNFeDetImpostoICMSICMS60 detImposto)
        {
            var cultureInfo = CultureInfo.InvariantCulture;
            var icms = new Entities.Imposto
            {
                TipoImposto = TipoImposto.Icms,
                Origem = detImposto.orig.ToOrigem(),
                CST = detImposto.CST.GetXmlAttrNameFromEnumValue(),
                BaseCalculoST = detImposto.vBCSTRet.ToDecimal(cultureInfo),
                AliquotaST = detImposto.pST.ToDecimal(cultureInfo),
                BaseCalculoFCP = detImposto.vBCFCPSTRet.ToDecimal(cultureInfo),
                AliquotaFCP = detImposto.pFCPSTRet.ToDecimal(cultureInfo)
            };

            return icms;
        }

        public static Entities.Imposto ToImposto(this TNFeInfNFeDetImpostoICMSICMS40 detImposto)
        {
            var valorDesonerado = detImposto.vICMSDeson.ToDecimal(CultureInfo.InvariantCulture);
            var icms = new Entities.Imposto
            {
                TipoImposto = TipoImposto.Icms,
                Origem = detImposto.orig.ToOrigem(),
                CST = detImposto.CST.GetXmlAttrNameFromEnumValue(),
                MotivoDesoneracao = valorDesonerado == 0 ? null : detImposto.motDesICMS.ToMotivoDesoneracao(),
                ValorDesonerado = valorDesonerado
            };
            return icms;
        }

        public static Entities.Imposto ToImposto(this TNFeInfNFeDetImpostoICMSICMS10 detImposto)
        {
            throw new NotImplementedException();
        }

        public static MotivoDesoneracao ToMotivoDesoneracao(this TNFeInfNFeDetImpostoICMSICMS40MotDesICMS motDesIcms)
        {
            switch (motDesIcms)
            {
                case TNFeInfNFeDetImpostoICMSICMS40MotDesICMS.Item1:
                    return MotivoDesoneracao.Taxi;
                case TNFeInfNFeDetImpostoICMSICMS40MotDesICMS.Item3:
                    return MotivoDesoneracao.ProdutorAgropecuario;
                case TNFeInfNFeDetImpostoICMSICMS40MotDesICMS.Item4:
                    return MotivoDesoneracao.FrotistaLocadora;
                case TNFeInfNFeDetImpostoICMSICMS40MotDesICMS.Item5:
                    return MotivoDesoneracao.DiplomaticoConsultar;
                case TNFeInfNFeDetImpostoICMSICMS40MotDesICMS.Item6:
                    return MotivoDesoneracao.UtilitariosAmazoniaAreasLivreComercio;
                case TNFeInfNFeDetImpostoICMSICMS40MotDesICMS.Item7:
                    return MotivoDesoneracao.Suframa;
                case TNFeInfNFeDetImpostoICMSICMS40MotDesICMS.Item8:
                    return MotivoDesoneracao.VendaOrgaoPublico;
                case TNFeInfNFeDetImpostoICMSICMS40MotDesICMS.Item9:
                    return MotivoDesoneracao.Outros;
                case TNFeInfNFeDetImpostoICMSICMS40MotDesICMS.Item10:
                    return MotivoDesoneracao.DeficienteCondutor;
                case TNFeInfNFeDetImpostoICMSICMS40MotDesICMS.Item11:
                    return MotivoDesoneracao.DeficienteNaoCondutor;
                case TNFeInfNFeDetImpostoICMSICMS40MotDesICMS.Item16:
                    return MotivoDesoneracao.OlimpiadasRio2016;
                default:
                    throw new ArgumentOutOfRangeException(nameof(motDesIcms), motDesIcms, null);
            }
        }
    }
}
