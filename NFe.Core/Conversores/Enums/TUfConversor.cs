using NFe.Core.XmlSchemas.NfeAutorizacao.Envio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Retorno = NFe.Core.XmlSchemas.NfeAutorizacao.Retorno.NfeProc;

namespace NFe.Core.Utils.Conversores.Enums
{
    public static class TUfConversor
    {
        public static TUf ToTUf(string uf)
        {
            var enumList = Enum.GetValues(typeof(TUf)).Cast<TUf>();
            var enumValue = enumList.First(e => Enum.GetName(typeof(TUf), e) == uf.ToUpperInvariant());
            return enumValue;
        }

        public static string ToSiglaUf(TUf uf)
        {
            return Enum.GetName(typeof(TUf), uf);
        }

        public static string ToSiglaUf(Retorno.TUf uf)
        {
            return Enum.GetName(typeof(Retorno.TUf), uf);
        }
    }
}
