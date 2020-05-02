using System;
using System.Globalization;
using System.Threading.Tasks;
using NFe.Core.Entitities;
using NFe.Core.Interfaces;
using NFe.Core.NotasFiscais.Sefaz.NfeRecepcaoEvento;

namespace NFe.Core.NotasFiscais.Services
{
    public class CancelaNotaFiscalFacade : ICancelaNotaFiscalFacade
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

        public MensagemRetornoEventoCancelamento CancelarNotaFiscal(string ufEmitente,
            CodigoUfIbge codigoUf, string cnpjEmitente, string chaveNFe,
            string protocoloAutorizacao, Modelo modeloNota, string justificativa)
        {
            var resultadoCancelamento = _nfeCancelamento.CancelarNotaFiscal(ufEmitente, codigoUf,
                cnpjEmitente,
                chaveNFe,
                protocoloAutorizacao, modeloNota, justificativa);

            if (resultadoCancelamento.Status != StatusEvento.SUCESSO)
                return resultadoCancelamento;

            var notaFiscalEntity = _notaFiscalRepository.GetNotaFiscalByChave(chaveNFe);

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