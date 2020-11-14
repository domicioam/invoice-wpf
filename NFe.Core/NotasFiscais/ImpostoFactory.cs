using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NFe.Core.Cadastro.Imposto;
using NFe.Core.NotasFiscais.Entities;

namespace NFe.Core.NotasFiscais
{
    class ImpostoFactory
    {
        internal Imposto CreateImposto(Entities.Imposto imposto, decimal valorProduto)
        {
            OrigemMercadoria origem;

            switch (imposto.Origem)
            {
                case Origem.Nacional:
                    origem = OrigemMercadoria.Nacional;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            switch (imposto.TipoImposto)
            {
                case TipoImposto.Confins:
                    switch (imposto.CST)
                    {
                        case "01":
                            return new CofinsCumulativoNaoCumulativo((decimal)imposto.BaseCalculo , (decimal)imposto.Aliquota);
                    }
                    break;
                case TipoImposto.Icms:
                    switch (imposto.CST)
                    {
                        case "60":
                            return new IcmsCobradoAnteriormentePorSubstituicaoTributaria(0, new BaseCalculoIcms(valorProduto), (decimal)imposto.Aliquota, 0, new BaseCalculoFundoCombatePobreza(valorProduto), origem);
                        case "41":
                            var desoneracaoIcms = new DesoneracaoIcms(0, MotivoDesoneracao.NaoPreenchido);
                            return new IcmsNaoTributado(desoneracaoIcms, origem);
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
                    throw new ArgumentOutOfRangeException();
            }

            throw new NotImplementedException();
        }
    }
}
