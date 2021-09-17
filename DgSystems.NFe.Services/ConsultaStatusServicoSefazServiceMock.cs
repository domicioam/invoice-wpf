using NFe.Core.Cadastro.Certificado;
using NFe.Core.Interfaces;
using NFe.Core.NotasFiscais.Services;
using NFe.Core.XmlSchemas.NfeStatusServico2.Retorno;
using System.Security.Cryptography.X509Certificates;
using System.Xml;

namespace DgSystems.NFe.Services.UnitTests
{
    public class ConsultaStatusServicoSefazServiceMock : ConsultaStatusServicoSefazService
    {
        private readonly string cStat;
        public ConsultaStatusServicoSefazServiceMock(IEmitenteRepository emissorService, ICertificadoRepository certificadoService,
            CertificadoService certificateManager, string cStat) : base(emissorService, certificadoService, certificateManager)
        {
            this.cStat = cStat;
        }

        protected override TRetConsStatServ ExecuteConsulta(X509Certificate2 certificado, string endpoint, XmlNode node)
        {
            return new TRetConsStatServ() { cStat = cStat };
        }
    }
}
