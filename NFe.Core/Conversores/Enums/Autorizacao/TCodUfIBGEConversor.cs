using NFe.Core.XmlSchemas.NfeAutorizacao.Envio;
using System;
using System.Collections.Generic;
using System.Text;
using NFe.Core.NotaFiscal;

namespace NFe.Core.Utils.Conversores.Enums.Autorizacao
{
    public static class TCodUfIBGEConversor
    {
        public static TCodUfIBGE ToTCodUfIBGE(this CodigoUfIbge codigo)
        {
            switch (codigo)
            {
                case CodigoUfIbge.DF:
                    return TCodUfIBGE.Item53;

                default:
                    throw new ArgumentException();
            }
        }

        public static CodigoUfIbge ToCodigoUfIbge(this TCodUfIBGE codigo)
        {
            switch (codigo)
            {
                case TCodUfIBGE.Item53:
                    return CodigoUfIbge.DF;

                default:
                    throw new ArgumentException();
            }
        }
    }
}
