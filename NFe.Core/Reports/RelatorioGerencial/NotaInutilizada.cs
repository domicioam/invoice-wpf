using System;

namespace NFe.Core.Utils.PDF.RelatorioGerencial
{
    public class NotaInutilizada
    {
        public string Serie { get; set; }
        public string Numero { get; set; }
        public DateTime DataInutilizacao { get; set; }
        public string Protocolo { get; set; }
        public string Motivo { get; set; }
    }
}