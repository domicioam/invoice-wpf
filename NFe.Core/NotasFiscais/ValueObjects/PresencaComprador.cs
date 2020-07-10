using System.ComponentModel;

namespace NFe.Core.NotasFiscais
{
    public enum PresencaComprador
    {
        [Description("Não se Aplica")] NaoAplica,
        [Description("Presencial")] Presencial,

        [Description("Não Presencial, Internet")]
        NaoPresencialInternet,

        [Description("Não Presencial, Teleatendimento")]
        NaoPresencialTeleatendimento,
        [Description("Entrega a Domicílio")] NfceEntregaDomicilio,

        [Description("Não Presencial, Outros")]
        NaoPresencialOutros
    }
}