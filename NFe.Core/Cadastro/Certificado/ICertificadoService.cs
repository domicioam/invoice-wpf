using System.Security.Cryptography.X509Certificates;
using NFe.Core.Entitities;

namespace NFe.Core.Cadastro.Certificado
{
    public interface ICertificadoService
    {
        CertificadoEntity GetCertificado();
        X509Certificate2 GetX509Certificate2();
        void Salvar(CertificadoEntity certificado);
    }
}