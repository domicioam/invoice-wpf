using System;

namespace DgSystem.NFe.Reports
{
    [Serializable]
    public class Identificacao
    {
        public Identificacao(Chave chave, string numero, string serie, DateTime dataHoraEmissao, string linkConsultaChave, string mensagemInteresseContribuinte)
        {
            Chave = chave;
            Numero = numero;
            Serie = serie;
            DataHoraEmissao = dataHoraEmissao;
            LinkConsultaChave = linkConsultaChave;
            MensagemInteresseContribuinte = mensagemInteresseContribuinte;
        }

        public byte[] QrCodeImage { get; set; }
        public Chave Chave { get; }
        public string Numero { get; }
        public string Serie { get; }
        public DateTime DataHoraEmissao { get; }
        public string LinkConsultaChave { get; }
        public string MensagemInteresseContribuinte { get; }
    }
}