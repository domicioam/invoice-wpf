using DgSystems.NFe.Services.Actors;
using NFe.Core;
using NFe.Core.Cadastro.Certificado;
using NFe.Core.Domain;
using NFe.Core.Interfaces;
using NFe.Core.NotasFiscais;
using NFe.Core.NotasFiscais.Sefaz.NfeAutorizacao;
using NFe.Core.NotasFiscais.Sefaz.NfeConsulta2;
using NFe.Core.Sefaz;
using System.Collections.Generic;

namespace DgSystems.NFe.Core.UnitTests.Services.Actors
{
    public class EmiteNFeContingenciaActorMock : EmiteNFeContingenciaActor
    {
        private readonly TipoMensagem tipoMensagem;

        public EmiteNFeContingenciaActorMock(INotaFiscalRepository notaFiscalRepository, IEmitenteRepository emissorService, IConsultarNotaFiscalService nfeConsulta, IServiceFactory serviceFactory, ICertificadoService certificadoService, SefazSettings sefazSettings, TipoMensagem erroValidacao)
            : base(notaFiscalRepository, emissorService, nfeConsulta, serviceFactory, certificadoService, sefazSettings)
        {
            this.tipoMensagem = erroValidacao;
        }

        public override MensagemRetornoTransmissaoNotasContingencia TransmitirLoteNotasFiscaisContingencia(List<string> nfeList, Modelo modelo)
        {
            return new MensagemRetornoTransmissaoNotasContingencia { TipoMensagem = tipoMensagem };
        }
    }
}
