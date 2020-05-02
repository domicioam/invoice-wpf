using System;
using System.Collections.Generic;
using System.Globalization;
using NFe.Core.Entitities;
using NFe.Core.Interfaces;
using NFe.Core.NotasFiscais;
using NFe.Core.NotasFiscais.Sefaz.NfeInutilizacao2;
using NFe.Core.NotasFiscais.Services;
using NFe.Core.Utils.Xml;
using NFe.Core.XmlSchemas.NfeInutilizacao2.Retorno.ProcInut;
using Status = NFe.Core.NotasFiscais.Sefaz.NfeInutilizacao2.Status;

namespace NFe.Core.Sefaz.Facades
{
    public class InutilizarNotaFiscalFacade
    {
        private NFeInutilizacao _nfeInutilizacao;
        private INotaInutilizadaService _notaInutilizadaService;
        private SefazSettings _sefazSettings;

        public virtual MensagemRetornoInutilizacao InutilizarNotaFiscal(string ufEmitente, CodigoUfIbge codigoUf, string cnpjEmitente, Modelo modeloNota,
            string serie, string numeroInicial, string numeroFinal)
        {
            // Pegar ambiente no App.Config
            var mensagemRetorno = _nfeInutilizacao.InutilizarNotaFiscal(ufEmitente, codigoUf, _sefazSettings.Ambiente, cnpjEmitente,
                modeloNota,
                serie, numeroInicial, numeroFinal);

            if (mensagemRetorno.Status != Status.ERRO)
            {
                var notaInutilizada = new NotaInutilizadaTO
                {
                    DataInutilizacao = DateTime.Now,
                    IdInutilizacao = mensagemRetorno.IdInutilizacao,
                    Modelo = modeloNota == Modelo.Modelo55 ? 55 : 65,
                    Motivo = mensagemRetorno.MotivoInutilizacao,
                    Numero = numeroInicial,
                    Protocolo = mensagemRetorno.ProtocoloInutilizacao,
                    Serie = serie
                };

                _notaInutilizadaService.Salvar(notaInutilizada, mensagemRetorno.Xml);
            }

            return mensagemRetorno;
        }

        public InutilizarNotaFiscalFacade(NFeInutilizacao nfeInutilizacao, INotaInutilizadaService notaInutilizadaService, SefazSettings sefazSettings)
        {
            _nfeInutilizacao = nfeInutilizacao;
            _notaInutilizadaService = notaInutilizadaService;
            _sefazSettings = sefazSettings;
        }

        protected InutilizarNotaFiscalFacade()
        {

        }
    }
}
