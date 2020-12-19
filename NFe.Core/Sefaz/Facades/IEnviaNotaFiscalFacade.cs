using NFe.Core.Sefaz.Facades;

namespace NFe.Core.NotasFiscais.Services
{
    public interface IEnviaNotaFiscalFacade
    {
        ResultadoEnvio EnviarNotaFiscal(NotaFiscal notaFiscal, string cscId, string csc);
    }
}