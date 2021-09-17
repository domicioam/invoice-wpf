using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace DgSystems.NFe.Services.UnitTests
{
    public class CertificateFixture
    {
        public X509Certificate2 X509Certificate2
        {
            get
            {
                return new X509Certificate2("MyDevCert.pfx", "SuperS3cret!");
            }
        }
    }
}
