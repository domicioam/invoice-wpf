using NFe.Core.Entitities;

namespace NFe.Core.Interfaces
{
    public interface ICertificadoRepository
    {
        int Salvar(CertificadoEntity certificado);
        void Excluir(CertificadoEntity certificado);
        CertificadoEntity GetCertificado();
    }
}