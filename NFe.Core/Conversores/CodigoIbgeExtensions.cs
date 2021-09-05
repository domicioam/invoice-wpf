using NFe.Core.Domain;
using NFe.Core.XmlSchemas.NfeRecepcaoEvento.Cancelamento.Envio;
using System;

namespace NFe.Core.Utils.Conversores
{
    public static class CodigoIbgeExtensions
    {
        public static TCOrgaoIBGE ToTCOrgaoIBGE(this CodigoUfIbge codigoUfIbge)
        {
            switch(codigoUfIbge)
            {
                case CodigoUfIbge.DF: return TCOrgaoIBGE.Item53;
                default: throw new NotSupportedException("Código não suportado.");
            }
        }
    }
}
