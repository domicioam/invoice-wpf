using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NFe.Core.Cadastro.Imposto;
using NFe.Core.Entitities.Enums;
using NFe.Core.XmlSchemas.NfeAutorizacao.Retorno.NfeProc;

namespace NFe.Core.NotasFiscais.Repositories
{
    internal static class ImpostoExtensions
    {
        public static Entities.Imposto ToImposto(this TNFeInfNFeDetImpostoICMSICMS60 detImposto)
        {
            var icms = new Entities.Imposto { TipoImposto = TipoImposto.Icms };

            icms.Origem = detImposto.orig.ToOrigem();
            if (detImposto != null)
            {
                icms.CST = TabelaIcmsCst.IcmsCobradoAnteriormentePorST;
            }

            return icms;
        }

        public static Entities.Imposto ToImposto(this TNFeInfNFeDetImpostoICMSICMS40 detImposto)
        {
            throw new NotImplementedException();
        }

        public static Entities.Imposto ToImposto(this TNFeInfNFeDetImpostoICMSICMS10 detImposto)
        {
            throw new NotImplementedException();
        }
    }
}
