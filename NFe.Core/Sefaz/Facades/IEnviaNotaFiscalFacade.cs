using NFe.Core.Sefaz.Facades;
using NFe.Core.NotaFiscal;

namespace NFe.Core.NotasFiscais.Services
{
    public interface IEnviaNotaFiscalFacade
    {
        ResultadoEnvio EnviarNotaFiscal(NotaFiscal.NotaFiscal notaFiscal, string cscId, string csc, System.Security.Cryptography.X509Certificates.X509Certificate2 certificado, XmlNFe xmlNFe);
    }
}