using NFe.Core.XmlSchemas.NfeAutorizacao.Envio;
using System;
using System.Collections.Generic;
using System.Text;
using Retorno = NFe.Core.XmlSchemas.NfeAutorizacao.Retorno.NfeProc;

namespace NFe.Core.Utils.Conversores.Enums
{
    public static class TUfConversor
    {
        public static TUf ToTUf(string uf)
        {
            switch (uf)
            {
                case "DF":
                case "df":
                    return TUf.DF;

                default:
                    throw new ArgumentException();
            }
        }

        public static string ToSiglaUf(TUf uf)
        {
            switch(uf)
            {
                case TUf.DF:
                    return "DF";

                default:
                    throw new NotImplementedException();
            }
        }

        public static string ToSiglaUf(Retorno.TUf uf)
        {
            switch (uf)
            {
                case Retorno.TUf.DF:
                    return "DF";

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
