using AutoMapper;
using NFe.Core.Cadastro.Certificado;
using NFe.Core.Entitities;
using NFe.Core.Interfaces;
using NFe.Core.NFeRecepcaoEvento4;
using NFe.Core.NotasFiscais.Sefaz.NfeRecepcaoEvento;
using NFe.Core.Sefaz;
using NFe.Core.Sefaz.Facades;
using NFe.Core.Utils.Assinatura;
using NFe.Core.Utils.Conversores;
using NFe.Core.XmlSchemas.NfeRecepcaoEvento.Cancelamento.Envio;
using NFe.Core.XmlSchemas.NfeRecepcaoEvento.Cancelamento.Retorno;
using System;
using System.Globalization;
using System.IO;
using System.Xml;
using Proc = NFe.Core.XmlSchemas.NfeRecepcaoEvento.Cancelamento.Retorno.Proc;
using TEvento = NFe.Core.XmlSchemas.NfeRecepcaoEvento.Cancelamento.Envio.TEvento;
using TEventoInfEvento = NFe.Core.XmlSchemas.NfeRecepcaoEvento.Cancelamento.Envio.TEventoInfEvento;
using TEventoInfEventoDetEvento = NFe.Core.XmlSchemas.NfeRecepcaoEvento.Cancelamento.Envio.TEventoInfEventoDetEvento;

namespace NFe.Core.NotasFiscais.Services
{
    public class CancelaNotaFiscalService : ICancelaNotaFiscalService
    {
        private const string NamespaceName = "http://www.portalfiscal.inf.br/nfe";
        private const string eventoVersao = "1.00";
        private readonly IEventoRepository eventoService;
        private readonly INotaFiscalRepository notaFiscalRepository;
        private readonly CertificadoService certificadoService;
        private readonly IServiceFactory serviceFactory;
        private readonly SefazSettings _sefazSettings;
        private readonly IMapper mapper;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public CancelaNotaFiscalService(INotaFiscalRepository notaFiscalRepository,
            IEventoRepository eventoService, CertificadoService certificadoService, IServiceFactory serviceFactory, SefazSettings sefazSettings, IMapper mapper)
        {
            this.notaFiscalRepository = notaFiscalRepository;
            this.eventoService = eventoService;
            this.certificadoService = certificadoService;
            this.serviceFactory = serviceFactory;
            _sefazSettings = sefazSettings;
            this.mapper = mapper;
        }

        public RetornoEventoCancelamento CancelarNotaFiscal(DadosNotaParaCancelar dadosNotaParaCancelar, string justificativa)
        {
            var resultadoCancelamento = CancelarNotaFiscalInternalMethod(dadosNotaParaCancelar, justificativa);
            if (resultadoCancelamento.Status != StatusEvento.SUCESSO)
                return resultadoCancelamento;

            var notaFiscalEntity = notaFiscalRepository.GetNotaFiscalByChave(dadosNotaParaCancelar.chaveNFe);
            eventoService.Salvar(new EventoEntity
            {
                DataEvento = DateTime.ParseExact(resultadoCancelamento.DataEvento, "yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture),
                TipoEvento = resultadoCancelamento.TipoEvento,
                Xml = resultadoCancelamento.Xml,
                NotaId = notaFiscalEntity.Id,
                ChaveIdEvento = resultadoCancelamento.IdEvento.Replace("ID", string.Empty),
                MotivoCancelamento = resultadoCancelamento.MotivoCancelamento,
                ProtocoloCancelamento = resultadoCancelamento.ProtocoloCancelamento
            });

            notaFiscalEntity.Status = (int)Status.CANCELADA;
            notaFiscalRepository.Salvar(notaFiscalEntity, null);
            return resultadoCancelamento;
        }

        private RetornoEventoCancelamento CancelarNotaFiscalInternalMethod(DadosNotaParaCancelar notaParaCancelar, string justificativa)
        {
            try
            {
                var infEvento = new TEventoInfEvento
                {
                    cOrgao = notaParaCancelar.codigoUf.ToTCOrgaoIBGE(),
                    tpAmb = (XmlSchemas.NfeRecepcaoEvento.Cancelamento.Envio.TAmb)(int)_sefazSettings.Ambiente,
                    Item = notaParaCancelar.cnpjEmitente,
                    ItemElementName = XmlSchemas.NfeRecepcaoEvento.Cancelamento.Envio.ItemChoiceType.CNPJ,
                    chNFe = notaParaCancelar.chaveNFe,
                    dhEvento = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                    tpEvento = TEventoInfEventoTpEvento.Item110111,
                    nSeqEvento = "1",
                    verEvento = TEventoInfEventoVerEvento.Item100,
                    detEvento = new TEventoInfEventoDetEvento()
                };
                infEvento.detEvento.versao = TEventoInfEventoDetEventoVersao.Item100;
                infEvento.detEvento.descEvento = TEventoInfEventoDetEventoDescEvento.Cancelamento;
                infEvento.detEvento.nProt = notaParaCancelar.protocoloAutorizacao;
                infEvento.detEvento.xJust = justificativa;
                infEvento.Id = "ID110111" + notaParaCancelar.chaveNFe + "01";

                var envioEvento = new TEnvEvento
                {
                    versao = eventoVersao,
                    idLote = "1",
                    evento = new TEvento[] { new TEvento { versao = eventoVersao, infEvento = infEvento } }
                };

                TRetEnvEvento retorno = ExecutaCancelamento(notaParaCancelar, infEvento, envioEvento);
                if (!retorno.cStat.Equals("128") || retorno.retEvento.Length == 0)
                {
                    return new RetornoEventoCancelamento()
                    {
                        Status = StatusEvento.ERRO,
                        Mensagem = "Erro desconhecido. Foi gerado um registro com o erro. Contate o suporte.",
                        Xml = ""
                    };
                }

                var retInfEvento = retorno.retEvento[0].infEvento;
                if (!retInfEvento.cStat.Equals("135"))
                    return new RetornoEventoCancelamento { Status = StatusEvento.ERRO, Mensagem = retInfEvento.xMotivo };

                var procEvento = new Proc.TProcEvento
                {
                    evento = mapper.Map<Proc.TEvento>(envioEvento.evento[0]),
                    retEvento = mapper.Map<Proc.TRetEvento>(retorno.retEvento[0]),
                    versao = "1.00"
                };

                return new RetornoEventoCancelamento()
                {
                    Status = StatusEvento.SUCESSO,
                    DataEvento = retInfEvento.dhRegEvento,
                    TipoEvento = retInfEvento.tpEvento,
                    Mensagem = retInfEvento.xMotivo,
                    Xml = XmlUtil.Serialize(procEvento, NamespaceName),
                    IdEvento = infEvento.Id,
                    MotivoCancelamento = justificativa,
                    ProtocoloCancelamento = retInfEvento.nProt
                };
            }
            catch (Exception e)
            {
                log.Error(e);
                string sDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "EmissorNFeDir");

                if (!Directory.Exists(sDirectory))
                    Directory.CreateDirectory(sDirectory);

                using (FileStream stream = File.Create(Path.Combine(sDirectory, "cancelarNotaErro.txt")))
                using (StreamWriter writer = new StreamWriter(stream))
                    writer.WriteLine(e.ToString());

                return new RetornoEventoCancelamento()
                {
                    Status = StatusEvento.ERRO,
                    Mensagem = "Erro ao tentar contactar SEFAZ. Verifique sua conexão.",
                    Xml = ""
                };
            }
        }

        protected virtual TRetEnvEvento ExecutaCancelamento(DadosNotaParaCancelar notaParaCancelar, TEventoInfEvento infEvento, TEnvEvento envioEvento)
        {
            string xml = XmlUtil.Serialize(envioEvento, NamespaceName);
            var certificado = certificadoService.GetX509Certificate2();
            XmlNode node = AssinaturaDigital.AssinarEvento(xml, "#" + infEvento.Id, certificado);
            //var resultadoValidacao = ValidadorXml.ValidarXml(node.OuterXml, "envEventoCancNFe_v1.00.xsd");

            var servico = serviceFactory.GetService(notaParaCancelar.modeloNota, Servico.CANCELAMENTO, notaParaCancelar.codigoUf, certificado);
            var client = servico.SoapClient as NFeRecepcaoEvento4SoapClient;
            var result = client.nfeRecepcaoEvento(node);
            return XmlUtil.Deserialize<TRetEnvEvento>(result.OuterXml) as TRetEnvEvento;
        }
    }
}