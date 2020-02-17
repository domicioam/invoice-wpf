using NFe.Core.XmlSchemas.NfeRecepcaoEvento.Cancelamento.Envio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFe.Core.Utils.Conversores
{
    public class UfToTCOrgaoIBGEConversor
    {
        public static TCOrgaoIBGE GetTCOrgaoIBGE(string uf)
        {
            switch (uf.ToUpperInvariant())
            {
                case "DF":
                    return TCOrgaoIBGE.Item53;

                default:
                    throw new ArgumentException("Argumento inválido.");
            }
        }
    }
}
