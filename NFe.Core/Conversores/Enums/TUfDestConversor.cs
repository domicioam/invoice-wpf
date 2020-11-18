using NFe.Core.XmlSchemas.NfeAutorizacao.Envio;
using System;
using System.Collections.Generic;
using System.Text;

namespace NFe.Core.Utils.Conversores.Enums
{
    public static class TUfDestConversor
    {
        public static TUf TUf(string uf)
        {
            switch (uf)
            {
                case "DF":
                case "df":
                    return XmlSchemas.NfeAutorizacao.Envio.TUf.DF;

                default:
                    throw new ArgumentException();
            }
        }

        public static string ToUfString(TUf uf)
        {
            switch (uf)
            {
                case XmlSchemas.NfeAutorizacao.Envio.TUf.DF:
                    return "DF";

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
