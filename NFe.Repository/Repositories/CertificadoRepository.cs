using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NFe.Core.Entitities;
using NFe.Core.Interfaces;

namespace NFe.Repository.Repositories
{
    public class CertificadoRepository : ICertificadoRepository
    {
        private NFeContext _context;

        public CertificadoRepository()
        {
            _context = new NFeContext();
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
