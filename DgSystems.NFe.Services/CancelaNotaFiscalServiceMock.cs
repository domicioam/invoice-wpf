using AutoMapper;
using NFe.Core.Cadastro.Certificado;
using NFe.Core.Interfaces;
using NFe.Core.NotasFiscais;
using NFe.Core.NotasFiscais.Services;
using NFe.Core.Sefaz;
using NFe.Core.Sefaz.Facades;
using NFe.Core.XmlSchemas.NfeRecepcaoEvento.Cancelamento.Envio;
using NFe.Core.XmlSchemas.NfeRecepcaoEvento.Cancelamento.Retorno;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DgSystems.NFe.Services.UnitTests
{
    class CancelaNotaFiscalServiceMock : CancelaNotaFiscalService
    {
        private readonly string cStat;
        private readonly TRetEvento[] retEventos;

        public CancelaNotaFiscalServiceMock(INotaFiscalRepository notaFiscalRepository, IEventoRepository eventoService,
            CertificadoService certificadoService, IServiceFactory serviceFactory, SefazSettings sefazSettings,
            IMapper mapper, string cStat, TRetEvento[] retEventos) : base(notaFiscalRepository, eventoService, certificadoService, serviceFactory, sefazSettings, mapper)
        {
            this.cStat = cStat;
            this.retEventos = retEventos;
        }

        protected override TRetEnvEvento ExecutaCancelamento(DadosNotaParaCancelar notaParaCancelar, TEventoInfEvento infEvento, TEnvEvento envioEvento)
        {
            return new TRetEnvEvento { cStat = cStat, retEvento = retEventos };
        }
    }
}
