using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using NFe.Core;
using NFe.Core.Interfaces;
using NFe.Core.Utils;
using NFe.Core.Utils.Assinatura;

namespace NFe.Core.Cadastro.Certificado
{
    public class CertificadoRepository : ICertificadoRepository
    {
        private NFeContext _context;
        private readonly ICertificateManager _certificateManager;
        private readonly RijndaelManagedEncryption _encryptor;

        public CertificadoRepository(ICertificateManager certificateManager, RijndaelManagedEncryption encryptor)
        {
            _context = new NFeContext();
            _certificateManager = certificateManager;
            _encryptor = encryptor;
        }

        public int Salvar(CertificadoEntity certificado)
        {
            if (certificado.Id == 0)
            {
                _context.Certificado.Add(certificado);
            }

            _context.SaveChanges();
            return certificado.Id;
        }

        public void Excluir(CertificadoEntity certificado)
        {
            _context.Certificado.Remove(certificado);
            _context.SaveChanges();
        }

        public CertificadoEntity GetCertificado()
        {
            return _context.Certificado.OrderByDescending(o => o.Id).FirstOrDefault();
        }

        public X509Certificate2 PickCertificateBasedOnInstallationType()
        {
            var certificadoEntity = GetCertificado();
            X509Certificate2 certificado;
            if (!string.IsNullOrWhiteSpace(certificadoEntity.Caminho))
            {

                certificado = _certificateManager.GetCertificateByPath(certificadoEntity.Caminho,
                    _encryptor.DecryptRijndael(certificadoEntity.Senha));
            }
            else
            {
                certificado = _certificateManager.GetCertificateBySerialNumber(certificadoEntity.NumeroSerial, false);
            }

            return certificado;
        }
    }
}
