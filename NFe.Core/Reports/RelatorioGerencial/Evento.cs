using System;

namespace NFe.Core.Utils.PDF.RelatorioGerencial
{
    public class Evento
    {
        public string TipoEvento { get; set; }
        public DateTime DataEvento { get; set; }
        public string XmlPath { get; set; }
        public int NotaId { get; set; }
        public string ChaveIdEvento { get; set; }
        public string MotivoCancelamento { get; set; }
        public string ProtocoloCancelamento { get; set; }
    }
}