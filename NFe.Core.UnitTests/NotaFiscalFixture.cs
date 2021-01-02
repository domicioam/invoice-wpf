using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using NFe.Core.Cadastro.Certificado;

namespace DgSystems.NFe.Core.UnitTests
{
    public class NotaFiscalFixture
    {
        public string NfeNamespaceName => "http://www.portalfiscal.inf.br/nfe";
        public string CscId => "000001";
        public string Csc => "E3BB2129-7ED0-31A10-CCB8-1B8BAC8FA2D0";
        public CertificadoEntity CertificadoEntity
        {
            get
            {
                return new CertificadoEntity
                {
                    Caminho = "MyDevCert.pfx",
                    Nome = "MOCK NAME",
                    NumeroSerial = "1234",
                    Senha = "VqkVinLLG4/EAKUokpnVDg=="
                };
            }
        }

        public X509Certificate2 X509Certificate2 {
            get
            {
                return new X509Certificate2("MyDevCert.pfx", "SuperS3cret!");
            }
        }
    }
}
