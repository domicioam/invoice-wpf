using NFe.Core.XmlSchemas.NfeAutorizacao.Envio;
using System;
using System.Collections.Generic;
using System.Text;

namespace NFe.Core.Utils.Conversores.Enums
{
    public static class TUfEmiConversor
    {
        public static TUfEmi TUfEmi(string uf)
        {
            switch (uf)
            {
                case "DF":
                case "df":
                    return XmlSchemas.NfeAutorizacao.Envio.TUfEmi.DF;

                default:
                    throw new ArgumentException();
            }
        }

        public static string ToUfString(TUfEmi uf)
        {
            switch(uf)
            {
                case XmlSchemas.NfeAutorizacao.Envio.TUfEmi.DF:
                    return "DF";

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
