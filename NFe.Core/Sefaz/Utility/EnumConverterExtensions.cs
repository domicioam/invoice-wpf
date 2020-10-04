using NFe.Core.XmlSchemas.NfeAutorizacao.Envio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NFe.Core.Icms;

namespace NFe.Core.Sefaz.Utility
{
    static class EnumConverterExtensions
    {
        /// <summary>
        /// Converte OrigemMercadoria para os valores de Torig
        /// </summary>
        /// <param name="origemMercadoria"></param>
        /// <returns></returns>
        public static Torig ToTorig(this OrigemMercadoria origemMercadoria)
        {
            switch (origemMercadoria)
            {
                case OrigemMercadoria.Nacional:
                    return Torig.Item0;
                case OrigemMercadoria.EstrangeiraImportacaoDireta:
                    return Torig.Item1;
                case OrigemMercadoria.EstrangeiraMercadoInterno:
                    return Torig.Item2;
                default:
                    throw new InvalidOperationException($"Origem de mercadoria não suportada: {origemMercadoria}.");
            }
        }

        /// <summary>
        /// Converte CstEnum para TNFeInfNFeDetImpostoICMSICMS60CST
        /// </summary>
        /// <param name="cst"></param>
        /// <returns></returns>
        public static TNFeInfNFeDetImpostoICMSICMS60CST ToTNFeInfNFeDetImpostoICMSICMS60CST(this CstEnum cst)
        {
            switch (cst)
            {
                case CstEnum.CST60:
                    return TNFeInfNFeDetImpostoICMSICMS60CST.Item60;
                default:
                    throw new InvalidOperationException($"CST não suportado para ICMS 60.");
            }
        }
    }
}
