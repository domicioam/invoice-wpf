using NFe.Core.Cadastro.Certificado;
using System.Security.Cryptography.X509Certificates;

namespace NFe.Core.Interfaces
{
    public interface ICertificadoRepository
    {
        int Salvar(CertificadoEntity certificado);
        void Excluir(CertificadoEntity certificado);
        CertificadoEntity GetCertificado();
        X509Certificate2 PickCertificateBasedOnInstallationType();
    }
}