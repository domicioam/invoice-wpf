using NFe.Core.XmlSchemas.NfeAutorizacao.Envio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NFe.Core.Icms;

namespace NFe.Core.Sefaz.Utility
{

    // TODO: Usar enums as classes e adicionar os converters dentro da classe e remover tudo que está aqui.

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

        /// <summary>
        /// Converte CstEnum para TNFeInfNFeDetImpostoICMSICMS40CST
        /// </summary>
        /// <param name="cst"></param>
        /// <returns></returns>
        public static TNFeInfNFeDetImpostoICMSICMS40CST ToTNFeInfNFeDetImpostoICMSICMS40CST(this CstEnum cst)
        {
            switch (cst)
            {
                case CstEnum.CST40:
                    return TNFeInfNFeDetImpostoICMSICMS40CST.Item40;
                case CstEnum.CST41:
                    return TNFeInfNFeDetImpostoICMSICMS40CST.Item41;
                default:
                    throw new InvalidOperationException($"CST não suportado para ICMS 40.");
            }
        }

        // ToTNFeInfNFeDetImpostoICMSICMS40MotDesICMS

        /// <summary>
        /// Converte CstEnum para TNFeInfNFeDetImpostoICMSICMS40CST
        /// </summary>
        /// <param name="cst"></param>
        /// <returns></returns>
        public static TNFeInfNFeDetImpostoICMSICMS40MotDesICMS ToTNFeInfNFeDetImpostoICMSICMS40MotDesICMS(this MotivoDesoneracao motivo)
        {
            switch (motivo)
            {

                case MotivoDesoneracao.Taxi:
                    return TNFeInfNFeDetImpostoICMSICMS40MotDesICMS.Item1;
                case MotivoDesoneracao.ProdutorAgropecuario:
                    return TNFeInfNFeDetImpostoICMSICMS40MotDesICMS.Item3;
                case MotivoDesoneracao.FrotistaLocadora:
                    return TNFeInfNFeDetImpostoICMSICMS40MotDesICMS.Item4;
                case MotivoDesoneracao.DiplomaticoConsultar:
                    return TNFeInfNFeDetImpostoICMSICMS40MotDesICMS.Item5;
                case MotivoDesoneracao.UtilitariosAmazoniaAreasLivreComercio:
                    return TNFeInfNFeDetImpostoICMSICMS40MotDesICMS.Item6;
                case MotivoDesoneracao.Suframa:
                    return TNFeInfNFeDetImpostoICMSICMS40MotDesICMS.Item7;
                case MotivoDesoneracao.VendaOrgaoPublico:
                    return TNFeInfNFeDetImpostoICMSICMS40MotDesICMS.Item8;
                case MotivoDesoneracao.Outros:
                    return TNFeInfNFeDetImpostoICMSICMS40MotDesICMS.Item9;
                case MotivoDesoneracao.DeficienteCondutor:
                    return TNFeInfNFeDetImpostoICMSICMS40MotDesICMS.Item10;
                case MotivoDesoneracao.DeficienteNaoCondutor:
                    return TNFeInfNFeDetImpostoICMSICMS40MotDesICMS.Item11;
                case MotivoDesoneracao.OlimpiadasRio2016:
                    return TNFeInfNFeDetImpostoICMSICMS40MotDesICMS.Item16;
                default:
                    throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Converte CstEnum para TNFeInfNFeDetImpostoCOFINSCOFINSAliqCST
        /// </summary>
        /// <param name="cst"></param>
        /// <returns></returns>
        public static TNFeInfNFeDetImpostoCOFINSCOFINSAliqCST ToSefazCofinsCST(this CofinsBase.CstEnum cst)
        {
            switch (cst)
            {
                case CofinsBase.CstEnum.CST01:
                    return TNFeInfNFeDetImpostoCOFINSCOFINSAliqCST.Item01;
                case CofinsBase.CstEnum.CST02:
                    return TNFeInfNFeDetImpostoCOFINSCOFINSAliqCST.Item02;
                default:
                    throw new InvalidOperationException($"CST não suportado para Cofins.");
            }
        }
    }
}
