using NFe.Core.Domain;
using NFe.Core.Entitities;
using NFe.Core.NotasFiscais.Sefaz.NfeRecepcaoEvento;
using NFe.Core.Sefaz.Facades;
using NFe.Core.XmlSchemas.NfeRecepcaoEvento.Cancelamento.Retorno;
using System;
using System.Globalization;

namespace DgSystems.NFe.Services.UnitTests
{
    public class CancelaNotaFiscalFixture
    {
        private const string DATE_STRING_FORMAT = "yyyy-MM-ddTHH:mm:sszzz";
        private readonly string IdEvento;

        private RetornoEventoCancelamento ResultadoCancelamentoSucesso;

        public CancelaNotaFiscalFixture()
        {
            NotaParaCancelar = new DadosNotaParaCancelar(codigoUf: CodigoUfIbge.DF, cnpjEmitente: "1234", chaveNFe: "1234", 
                protocoloAutorizacao: "1234", modeloNota: Modelo.Modelo55, ufEmitente: "DF");

            IdEvento = "ID110111" + NotaParaCancelar.chaveNFe + "01";

            ResultadoCancelamentoSucesso = new RetornoEventoCancelamento()
            {
                Status = StatusEvento.SUCESSO,
                DataEvento = new DateTime(20, 10, 20).ToString(DATE_STRING_FORMAT),
                IdEvento = IdEvento
            };

            expectedEventoEntity = new EventoEntity
            {
                DataEvento = DateTime.ParseExact(ResultadoCancelamentoSucesso.DataEvento, DATE_STRING_FORMAT, CultureInfo.InvariantCulture),
                ChaveIdEvento = ResultadoCancelamentoSucesso.IdEvento.Replace("ID", string.Empty)
            };

        }

        public EventoEntity expectedEventoEntity;

        public string cStatSucesso => "128";
        public TRetEvento[] RetEventoSuceso
        {
            get
            {
                return new TRetEvento[]
                {
                    new TRetEvento
                    { 
                        infEvento = new TRetEventoInfEvento
                        {
                            cStat = "135", 
                            dhRegEvento = ResultadoCancelamentoSucesso.DataEvento, 
                            Id = IdEvento,
                            nProt = "1234"
                        }
                    }
                };
            }
        }

        public DadosNotaParaCancelar NotaParaCancelar { get; }
    }
}