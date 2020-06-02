using System.Security.Cryptography.X509Certificates;

namespace NFe.Core.Cadastro.Certificado
{
    public interface ICertificadoService
    {
        X509Certificate2 GetX509Certificate2();
    }
}