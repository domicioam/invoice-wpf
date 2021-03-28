using System;
using System.Globalization;
using System.Threading.Tasks;
using NFe.Core.Entitities;
using NFe.Core.Interfaces;
using NFe.Core.NotasFiscais.Sefaz.NfeRecepcaoEvento;
using NFe.Core.Sefaz.Facades;

namespace NFe.Core.NotasFiscais.Services
{
    public class CancelaNotaFiscalFacade : ICancelaNotaFiscalService
    {
        private readonly IEventoService _eventoService;
        private readonly INFeCancelamento _nfeCancelamento;
        private readonly INotaFiscalRepository _notaFiscalRepository;

        public CancelaNotaFiscalFacade(INFeCancelamento nfeCancelamento, INotaFiscalRepository notaFiscalRepository,
            IEventoService eventoService)
        {
            _nfeCancelamento = nfeCancelamento;
            _notaFiscalRepository = notaFiscalRepository;
            _eventoService = eventoService;
        }

        public MensagemRetornoEventoCancelamento CancelarNotaFiscal(DadosNotaParaCancelar dadosNotaParaCancelar, string justificativa)
        {
            var resultadoCancelamento = _nfeCancelamento.CancelarNotaFiscal(dadosNotaParaCancelar.ufEmitente, dadosNotaParaCancelar.codigoUf,
                dadosNotaParaCancelar.cnpjEmitente,
                dadosNotaParaCancelar.chaveNFe,
                dadosNotaParaCancelar.protocoloAutorizacao, dadosNotaParaCancelar.modeloNota, justificativa);

            if (resultadoCancelamento.Status != StatusEvento.SUCESSO)
                return resultadoCancelamento;

            var notaFiscalEntity = _notaFiscalRepository.GetNotaFiscalByChave(dadosNotaParaCancelar.chaveNFe);

            _eventoService.Salvar(new EventoEntity
            {
                DataEvento = DateTime.ParseExact(resultadoCancelamento.DataEvento, "yyyy-MM-ddTHH:mm:sszzz",
                    CultureInfo.InvariantCulture),
                TipoEvento = resultadoCancelamento.TipoEvento,
                Xml = resultadoCancelamento.Xml,
                NotaId = notaFiscalEntity.Id,
                ChaveIdEvento = resultadoCancelamento.IdEvento.Replace("ID", string.Empty),
                MotivoCancelamento = resultadoCancelamento.MotivoCancelamento,
                ProtocoloCancelamento = resultadoCancelamento.ProtocoloCancelamento
            });

            notaFiscalEntity.Status = (int) Status.CANCELADA;
             _notaFiscalRepository.Salvar(notaFiscalEntity, null);

            return resultadoCancelamento;
        }
    }
}