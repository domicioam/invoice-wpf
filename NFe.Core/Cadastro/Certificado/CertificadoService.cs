using System.IO;
using System.Security.Cryptography.X509Certificates;
using NFe.Core.Interfaces;
using NFe.Core.Utils;
using NFe.Core.Utils.Assinatura;

namespace NFe.Core.Cadastro.Certificado
{
    public class CertificadoService : ICertificadoService
    {
        private ICertificadoRepository _certificadoRepository;
        private ICertificateManager _certificateManager;
        private RijndaelManagedEncryption _encryptor;

        public CertificadoService(ICertificadoRepository certificadoRepository, ICertificateManager certificateManager, RijndaelManagedEncryption encryptor)
        {
            _certificadoRepository = certificadoRepository;
            _certificateManager = certificateManager;
            _encryptor = encryptor;
        }

        public X509Certificate2 GetX509Certificate2()
        {
            X509Certificate2 certificado;
            var certificadoEntity = _certificadoRepository.GetCertificado();

            if (certificadoEntity == null) return null;

            if (!string.IsNullOrWhiteSpace(certificadoEntity.Caminho))
                certificado = _certificateManager.GetCertificateByPath(certificadoEntity.Caminho,
                    _encryptor.DecryptRijndael(certificadoEntity.Senha));
            else
                certificado = _certificateManager.GetCertificateBySerialNumber(certificadoEntity.NumeroSerial, false);

            return certificado;
        }
    }
}