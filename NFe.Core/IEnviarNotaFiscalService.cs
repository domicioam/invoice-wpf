using NFe.Core.Sefaz.Facades;
using NFe.Core.Domain;

namespace NFe.Core.NotasFiscais.Services
{
    public interface IEnviaNotaFiscalService
    {
        ResultadoEnvio EnviarNotaFiscal(Domain.NotaFiscal notaFiscal, string cscId, string csc, System.Security.Cryptography.X509Certificates.X509Certificate2 certificado, XmlNFe xmlNFe);
    }
}