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
            //var icms = new Entities.Imposto
            //{
            //    Aliquota = detImposto.
            //};
            
            
            
            throw new NotImplementedException();
        }

        public static Entities.Imposto ToImposto(this TNFeInfNFeDetImpostoICMSICMS10 detImposto)
        {
            throw new NotImplementedException();
        }
        
        
    }
}
