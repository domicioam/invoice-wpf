using NFe.Core.Cadastro.Certificado;

namespace NFe.Core.Interfaces
{
    public interface ICertificadoRepository
    {
        int Salvar(CertificadoEntity certificado);
        void Excluir(CertificadoEntity certificado);
        CertificadoEntity GetCertificado();
    }
}