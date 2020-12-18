using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NFe.Core.Cadastro.Imposto;
using NFe.Core.NotasFiscais.Entities;
using static NFe.Core.Icms;

namespace NFe.Core.NotasFiscais
{
    class ImpostoFactory
    {
        internal Imposto CreateImposto(Entities.Imposto imposto)
        {
            OrigemMercadoria origem;

            switch (imposto.Origem)
            {
                case Origem.Nacional:
                case null:
                    origem = OrigemMercadoria.Nacional;
                    break;

                default:
                    throw new NotSupportedException();
            }

            switch (imposto.TipoImposto)
            {
                case TipoImposto.Cofins:
                    switch (imposto.CST)
                    {
                        case "01":
                            return new CofinsCumulativoNaoCumulativo((decimal)imposto.BaseCalculo, (decimal)imposto.Aliquota);
                    }
                    break;
                case TipoImposto.Icms:
                    switch (imposto.CST)
                    {
                        case "60":
                            return new IcmsCobradoAnteriormentePorSubstituicaoTributaria((decimal)imposto.Aliquota, (decimal)imposto.BaseCalculo, new FundoCombatePobreza(imposto.AliquotaFCP, imposto.BaseCalculoFCP), origem);
                        case "41":
                            Desoneracao desoneracaoIcms = null;
                            if (imposto.ValorDesonerado > 0 || imposto.MotivoDesoneracao != null)
                            {
                                desoneracaoIcms = new Desoneracao(imposto.ValorDesonerado, imposto.MotivoDesoneracao);
                            }
                            return new IcmsNaoTributado(desoneracaoIcms, origem, (decimal)imposto.Aliquota);
                        case "10":
                            return new Icms10((decimal)imposto.Aliquota, (decimal)imposto.BaseCalculo, CstEnum.CST10, origem, new FundoCombatePobreza(imposto.AliquotaFCP, imposto.BaseCalculoFCP), 
                                new Impostos.Icms.SubstituicaoTributaria(new FundoCombatePobreza(imposto.AliquotaFCP, imposto.BaseCalculoFCP), imposto.BaseCalculoST, imposto.AliquotaST));
                    }
                    break;
                case TipoImposto.IcmsST:
                    break;
                case TipoImposto.IPI:
                    break;
                case TipoImposto.PIS:
                    switch (imposto.CST)
                    {
                        case "04":
                            return new PisOperacaoTributavelMonofasica();
                        case "07":
                            return new PisOperacaoIsentaContribuicao();
                    }
                    break;
                default:
                    throw new NotSupportedException();
            }

            throw new NotSupportedException("");
        }
    }
}
