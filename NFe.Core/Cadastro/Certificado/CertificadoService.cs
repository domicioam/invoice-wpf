using System.IO;
using System.Security.Cryptography.X509Certificates;
using NFe.Core.Entitities;
using NFe.Core.Interfaces;
using NFe.Core.Utils;
using NFe.Core.Utils.Assinatura;

namespace NFe.Core.Cadastro.Certificado
{
    public class CertificadoService : ICertificadoService
    {
        private ICertificadoRepository _certificadoRepository;
        private ICertificateManager _certificateManager;

        public CertificadoService(ICertificadoRepository certificadoRepository, ICertificateManager certificateManager)
        {
            _certificadoRepository = certificadoRepository;
            _certificateManager = certificateManager;
        }

        public CertificadoEntity GetCertificado()
        {
            var certificado = _certificadoRepository.GetCertificado();

            if (File.Exists(certificado.Caminho)) return certificado;

            return null;
        }

        public void Salvar(CertificadoEntity certificado)
        {
            _certificadoRepository.Salvar(certificado);
        }

        public X509Certificate2 GetX509Certificate2()
        {
            X509Certificate2 certificado;
            var certificadoEntity = GetCertificado();

            if (certificadoEntity == null) return null;

            if (!string.IsNullOrWhiteSpace(certificadoEntity.Caminho))
                certificado = _certificateManager.GetCertificateByPath(certificadoEntity.Caminho,
                    RijndaelManagedEncryption.DecryptRijndael(certificadoEntity.Senha));
            else
                certificado = _certificateManager.GetCertificateBySerialNumber(certificadoEntity.NumeroSerial, false);

            return certificado;
        }
    }
}