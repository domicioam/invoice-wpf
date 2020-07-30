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
        internal Imposto CreateImposto(Entities.Imposto imposto)
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
                    break;
                case TipoImposto.Icms:
                    switch (imposto.CST)
                    {
                        case "60":
                            return new IcmsCobradoAnteriormentePorSubstituicaoTributaria(0, (decimal)imposto.Aliquota, (decimal)imposto.BaseCalculo, (decimal)imposto.Reducao, 0, 0, 0, 0, 0, 0, origem);
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
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }




            throw new NotImplementedException();
        }
    }
}
