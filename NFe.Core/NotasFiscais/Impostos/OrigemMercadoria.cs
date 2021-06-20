using NFe.Core.Extensions;
using NFe.Core.XmlSchemas.NfeAutorizacao.Envio;
using System;

namespace NFe.Core
{
    public class OrigemMercadoria : Enumeration
    {
        public static readonly OrigemMercadoria Nacional = new OrigemMercadoria(0, "Nacional");
        public static readonly OrigemMercadoria EstrangeiraImportacaoDireta = new OrigemMercadoria(1, "EstrangeiraImportacaoDireta");
        public static readonly OrigemMercadoria EstrangeiraMercadoInterno = new OrigemMercadoria(2, "EstrangeiraMercadoInterno");

        public OrigemMercadoria(int id, string name) : base(id, name)
        {
        }

        public static implicit operator Torig(OrigemMercadoria origemMercadoria)
        {
            if(origemMercadoria == Nacional)
            {
                return Torig.Item0;
            } else if(origemMercadoria == EstrangeiraImportacaoDireta)
            {
                return Torig.Item1;
            } else if (origemMercadoria == EstrangeiraMercadoInterno)
            {
                return Torig.Item2;
            } else
            {
                throw new InvalidOperationException($"Origem de mercadoria não suportada: {origemMercadoria}.");
            }
        }
    }
}
 