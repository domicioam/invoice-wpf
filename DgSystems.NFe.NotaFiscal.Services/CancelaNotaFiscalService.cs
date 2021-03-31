using System;
using System.Globalization;
using NFe.Core.Entitities;
using NFe.Core.Interfaces;
using NFe.Core.NotasFiscais.Sefaz.NfeRecepcaoEvento;
using NFe.Core.Sefaz.Facades;
using NFe.Core.NFeRecepcaoEvento4;
using NFe.Core.Domain;
using NFe.Core.Sefaz;
using NFe.Core.Utils.Assinatura;
using NFe.Core.Utils.Conversores;
using NFe.Core.XmlSchemas.NfeRecepcaoEvento.Cancelamento.Envio;
using NFe.Core.XmlSchemas.NfeRecepcaoEvento.Cancelamento.Retorno;
using Proc = NFe.Core.XmlSchemas.NfeRecepcaoEvento.Cancelamento.Retorno.Proc;
using TEvento = NFe.Core.XmlSchemas.NfeRecepcaoEvento.Cancelamento.Envio.TEvento;
using TEventoInfEvento = NFe.Core.XmlSchemas.NfeRecepcaoEvento.Cancelamento.Envio.TEventoInfEvento;
using TEventoInfEventoDetEvento = NFe.Core.XmlSchemas.NfeRecepcaoEvento.Cancelamento.Envio.TEventoInfEventoDetEvento;
using NFe.Core.Cadastro.Certificado;
using System.Xml;
using System.Linq;
using System.IO;

namespace NFe.Core.NotasFiscais.Services
{
    public class CancelaNotaFiscalService : ICancelaNotaFiscalService
    {
        private readonly IEventoService _eventoService;
        private readonly INFeCancelamento _nfeCancelamento;
        private readonly INotaFiscalRepository _notaFiscalRepository;
        private readonly ICertificadoService _certificadoService;
        private IServiceFactory _serviceFactory;
        private SefazSettings _sefazSettings;
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public CancelaNotaFiscalService(INFeCancelamento nfeCancelamento, INotaFiscalRepository notaFiscalRepository,
            IEventoService eventoService, ICertificadoService certificadoService, IServiceFactory serviceFactory, SefazSettings sefazSettings)
        {
            _nfeCancelamento = nfeCancelamento;
            _notaFiscalRepository = notaFiscalRepository;
            _eventoService = eventoService;
            _certificadoService = certificadoService;
            _serviceFactory = serviceFactory;
            _sefazSettings = sefazSettings;
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

            notaFiscalEntity.Status = (int)Status.CANCELADA;
            _notaFiscalRepository.Salvar(notaFiscalEntity, null);

            return resultadoCancelamento;
        }

        private MensagemRetornoEventoCancelamento CancelarNotaFiscal(string ufEmitente, CodigoUfIbge codigoUf, string cnpjEmitente, string chaveNFe,
            string protocoloAutorizacao, Modelo modeloNota, string justificativa)
        {
            try
            {
                var infEvento = new TEventoInfEvento();
                infEvento.cOrgao = UfToTCOrgaoIBGEConversor.GetTCOrgaoIBGE(ufEmitente);
                infEvento.tpAmb = (XmlSchemas.NfeRecepcaoEvento.Cancelamento.Envio.TAmb)(int)_sefazSettings.Ambiente;
                infEvento.Item = cnpjEmitente;
                infEvento.ItemElementName = XmlSchemas.NfeRecepcaoEvento.Cancelamento.Envio.ItemChoiceType.CNPJ;
                infEvento.chNFe = chaveNFe;
                infEvento.dhEvento = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz");
                infEvento.tpEvento = TEventoInfEventoTpEvento.Item110111;
                infEvento.nSeqEvento = "1";
                infEvento.verEvento = TEventoInfEventoVerEvento.Item100;

                infEvento.detEvento = new TEventoInfEventoDetEvento();
                infEvento.detEvento.versao = TEventoInfEventoDetEventoVersao.Item100;
                infEvento.detEvento.descEvento = TEventoInfEventoDetEventoDescEvento.Cancelamento;
                infEvento.detEvento.nProt = protocoloAutorizacao;
                infEvento.detEvento.xJust = justificativa;
                infEvento.Id = "ID110111" + chaveNFe + "01";

                var evento = new TEvento();
                evento.versao = "1.00";
                evento.infEvento = infEvento;

                var envioEvento = new TEnvEvento();
                envioEvento.versao = "1.00";
                envioEvento.idLote = "1";
                envioEvento.evento = new TEvento[] { evento };

                var xml = XmlUtil.Serialize(envioEvento, "http://www.portalfiscal.inf.br/nfe");

                var certificado = _certificadoService.GetX509Certificate2();

                XmlNode node = AssinaturaDigital.AssinarEvento(xml, "#" + infEvento.Id, certificado);

                //var resultadoValidacao = ValidadorXml.ValidarXml(node.OuterXml, "envEventoCancNFe_v1.00.xsd");

                var servico = _serviceFactory.GetService(modeloNota,
                                                Servico.CANCELAMENTO, codigoUf, certificado);

                var client = (NFeRecepcaoEvento4SoapClient)servico.SoapClient;

                var result = client.nfeRecepcaoEvento(node);

                var retorno = (TRetEnvEvento)XmlUtil.Deserialize<TRetEnvEvento>(result.OuterXml);

                if (retorno.cStat.Equals("128"))
                {
                    var retEvento = retorno.retEvento;

                    if (retEvento.Count() > 0)
                    {
                        var retInfEvento = retEvento[0].infEvento;

                        if (retInfEvento.cStat.Equals("135"))
                        {
                            var procEvento = new Proc.TProcEvento();

                            TEnvEvento envEvento = (TEnvEvento)XmlUtil.Deserialize<TEnvEvento>(node.OuterXml);
                            var eventoSerialized = XmlUtil.Serialize(envEvento.evento[0], "");
                            procEvento.evento = (Proc.TEvento)XmlUtil.Deserialize<Proc.TEvento>(eventoSerialized);

                            var retEventoSerialized = XmlUtil.Serialize(retEvento[0], "");
                            procEvento.retEvento = (Proc.TRetEvento)XmlUtil.Deserialize<Proc.TRetEvento>(retEventoSerialized);

                            procEvento.versao = "1.00";
                            var procSerialized = XmlUtil.Serialize(procEvento, "http://www.portalfiscal.inf.br/nfe");

                            return new MensagemRetornoEventoCancelamento()
                            {
                                Status = StatusEvento.SUCESSO,
                                DataEvento = retInfEvento.dhRegEvento,
                                TipoEvento = retInfEvento.tpEvento,
                                Mensagem = retInfEvento.xMotivo,
                                Xml = procSerialized,
                                IdEvento = infEvento.Id,
                                MotivoCancelamento = justificativa,
                                ProtocoloCancelamento = retInfEvento.nProt
                            };
                        }
                        else
                        {
                            return new MensagemRetornoEventoCancelamento()
                            {
                                Status = StatusEvento.ERRO,
                                Mensagem = retInfEvento.xMotivo,
                            };
                        }
                    }
                }

                return new MensagemRetornoEventoCancelamento()
                {
                    Status = StatusEvento.ERRO,
                    Mensagem = "Erro desconhecido. Foi gerado um registro com o erro. Contate o suporte.",
                    Xml = ""
                };
            }
            catch (Exception e)
            {
                log.Error(e);
                string sDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "EmissorNFeDir");

                if (!Directory.Exists(sDirectory))
                {
                    Directory.CreateDirectory(sDirectory);
                }

                using (FileStream stream = File.Create(Path.Combine(sDirectory, "cancelarNotaErro.txt")))
                {
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        writer.WriteLine(e.ToString());
                    }
                }

                return new MensagemRetornoEventoCancelamento()
                {
                    Status = StatusEvento.ERRO,
                    Mensagem = "Erro ao tentar contactar SEFAZ. Verifique sua conexão.",
                    Xml = ""
                };
            }
        }
    }
}