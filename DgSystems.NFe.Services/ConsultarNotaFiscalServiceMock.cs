using NFe.Core.NotasFiscais.Sefaz.NfeConsulta2;
using NFe.Core.Sefaz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using NFe.Core.XmlSchemas.NfeConsulta2.Retorno;

namespace DgSystems.NFe.Services.UnitTests
{
    class ConsultarNotaFiscalServiceMock : ConsultarNotaFiscalService
    {
        private string cStat;

        public ConsultarNotaFiscalServiceMock(SefazSettings sefazSettings, string cStat) : base(sefazSettings)
        {
            this.cStat = cStat;
        }

        protected override TRetConsSitNFe ExecutaConsulta(X509Certificate2 certificado, string endpoint, System.Xml.XmlNode node)
        {
            return new TRetConsSitNFe() { cStat = cStat, protNFe = new TProtNFe() { infProt = new TProtNFeInfProt() } };
        }
    }
}
