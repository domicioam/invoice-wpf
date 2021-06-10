using System;

namespace DgSystem.NFe.Reports.Nfe
{
    [Serializable]
    public class Identificacao
    {
        public Identificacao(Chave chave, string numero, string serie, DateTime dataHoraEmissao, string linkConsultaChave, string mensagemInteresseContribuinte, string dataSaida, string horaSaida, string naturezaOperacao, string tipoOperacaooTexto)
        {
            Chave = chave;
            Numero = numero;
            Serie = serie;
            DataHoraEmissao = dataHoraEmissao;
            LinkConsultaChave = linkConsultaChave;
            MensagemInteresseContribuinte = mensagemInteresseContribuinte;
            DataSaida = dataSaida;
            HoraSaida = horaSaida;
            NaturezaOperacao = naturezaOperacao;
            TipoOperacaooTexto = tipoOperacaooTexto;
        }

        public byte[] QrCodeImage { get; set; }
        public Chave Chave { get; }
        public string Numero { get; }
        public string Serie { get; }
        public DateTime DataHoraEmissao { get; }
        public string LinkConsultaChave { get; }
        public string MensagemInteresseContribuinte { get; }
        public string DataSaida { get; internal set; }
        public string HoraSaida { get; internal set; }
        public string NaturezaOperacao { get; internal set; }
        public string TipoOperacaooTexto { get; internal set; }
    }
}