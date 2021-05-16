namespace DgSystem.NFe.Reports
{
    public class Identificacao
    {
        public byte[] QrCodeImage { get; internal set; }
        public Chave Chave { get; internal set; }
        public object Numero { get; internal set; }
        public object Serie { get; internal set; }
        public object DataHoraEmissao { get; internal set; }
        public object LinkConsultaChave { get; internal set; }
        public object MensagemInteresseContribuinte { get; internal set; }
    }
}