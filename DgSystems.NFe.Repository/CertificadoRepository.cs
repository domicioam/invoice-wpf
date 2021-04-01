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
        private readonly RijndaelManagedEncryption _encryptor;

        public CertificadoRepository(RijndaelManagedEncryption encryptor)
        {
            _context = new NFeContext();
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


    }
}
