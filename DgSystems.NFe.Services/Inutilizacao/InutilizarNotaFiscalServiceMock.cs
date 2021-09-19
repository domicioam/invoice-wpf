using NFe.Core.Cadastro.Certificado;
using NFe.Core.Domain;
using NFe.Core.Interfaces;
using NFe.Core.NotasFiscais;
using NFe.Core.Sefaz;
using NFe.Core.Sefaz.Facades;
using NFe.Core.XmlSchemas.NfeInutilizacao2.Envio;
using NFe.Core.XmlSchemas.NfeInutilizacao2.Retorno;
using System.Xml;

namespace DgSystems.NFe.Services.UnitTests
{
    internal class InutilizarNotaFiscalServiceMock : InutilizarNotaFiscalService
    {
        private readonly string cStat;

        public InutilizarNotaFiscalServiceMock(INotaInutilizadaRepository notaInutilizadaService,
            SefazSettings sefazSettings, CertificadoService certificadoService, IServiceFactory serviceFactory, string cStat) : base(notaInutilizadaService, sefazSettings, certificadoService, serviceFactory)
        {
            this.cStat = cStat;
        }

        protected override (XmlDocument node, XmlNode result, TRetInutNFe retorno) ExecuteInutilizacao(CodigoUfIbge codigoUf, Modelo modeloNota, TInutNFeInfInut infInut)
        {
            var node = new XmlDocument();
            var retorno = new TRetInutNFe { 
                infInut = new TRetInutNFeInfInut
                {
                    cStat = cStat,
                    xMotivo = "123",
                    dhRecbto = "123",
                    nProt = "123"
                }
            };

            return (node, node, retorno);
        }
    }
}