namespace NFe.Core.NotasFiscais.Sefaz.NfeRecepcaoEvento
{
    public class RetornoEventoCancelamento
    {
        public StatusEvento Status { get; set; }
        public string Mensagem { get; set; }
        public string Xml { get; set; }
        public string DataEvento { get; set; }
        public string TipoEvento { get; set; }
        public string IdEvento { get; set; }
        public string ProtocoloCancelamento { get; set; }
        public string MotivoCancelamento { get; set; }
    }
}
