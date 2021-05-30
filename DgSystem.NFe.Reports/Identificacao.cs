using System;

namespace DgSystem.NFe.Reports
{
    public class Identificacao
    {
        public byte[] QrCodeImage { get; internal set; }
        public Chave Chave { get; internal set; }
        public string Numero { get; internal set; }
        public string Serie { get; internal set; }
        public DateTime DataHoraEmissao { get; internal set; }
        public string LinkConsultaChave { get; internal set; }
        public string MensagemInteresseContribuinte { get; internal set; }
    }
}