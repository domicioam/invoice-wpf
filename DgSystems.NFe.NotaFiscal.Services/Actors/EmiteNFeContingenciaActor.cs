using Akka.Actor;
using NFe.Core;
using NFe.Core.Cadastro.Certificado;
using NFe.Core.Domain;
using NFe.Core.Entitities;
using NFe.Core.Interfaces;
using NFe.Core.NFeAutorizacao4;
using NFe.Core.NFeRetAutorizacao4;
using NFe.Core.NotasFiscais;
using NFe.Core.NotasFiscais.Sefaz.NfeAutorizacao;
using NFe.Core.NotasFiscais.Sefaz.NfeConsulta2;
using NFe.Core.Sefaz;
using NFe.Core.Utils.Xml;
using NFe.Core.XmlSchemas.NfeAutorizacao.Envio;
using NFe.Core.XmlSchemas.NfeAutorizacao.Retorno;
using NFe.Core.XmlSchemas.NfeRetAutorizacao.Envio;
using NFe.Core.XmlSchemas.NfeRetAutorizacao.Retorno;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Status = NFe.Core.Entitities.Status;
using Consulta = NFe.Core.XmlSchemas.NfeConsulta2.Retorno;
using AutoMapper;
using RetAutorizacao = NFe.Core.XmlSchemas.NfeRetAutorizacao;
using NFe.Core.Cadastro.Configuracoes;

namespace DgSystems.NFe.Services.Actors
{
    public class EmiteNFeContingenciaActor : ReceiveActor
    {
        #region Messages

        public class TransmitirNFeEmContingencia { }
        public class SaveNotaFiscalContingencia
        {
            public SaveNotaFiscalContingencia(X509Certificate2 certificado, ConfiguracaoEntity config, NotaFiscal notaFiscal, string cscId, string csc, string nFeNamespaceName)
            {
                this.certificado = certificado;
                this.config = config;
                this.notaFiscal = notaFiscal;
                this.cscId = cscId;
                this.csc = csc;
                this.nFeNamespaceName = nFeNamespaceName;
            }

            public X509Certificate2 certificado { get; }
            public ConfiguracaoEntity config { get; }
            public NotaFiscal notaFiscal { get; }
            public string cscId { get; }
            public string csc { get; }
            public string nFeNamespaceName { get; }

        }

        private class TransmiteNFes
        {
            public TransmiteNFes(List<string> nfe, Modelo modelo)
            {
                Modelo = modelo;
                Nfe = nfe;
            }

            public Modelo Modelo { get; }
            public List<string> Nfe { get; }
        }

        private class ConsultaRecibos
        {
            public ConsultaRecibos(int tempoEspera, MensagemRetornoTransmissaoNotasContingencia mensagemRetorno, Modelo modelo)
            {
                MensagemRetorno = mensagemRetorno;
                TempoEspera = tempoEspera;
                Modelo = modelo;
            }

            public MensagemRetornoTransmissaoNotasContingencia MensagemRetorno { get; }
            public int TempoEspera { get; }
            public Modelo Modelo { get; }
        }

        private class ValidaNFesTransmitidas
        {
            public ValidaNFesTransmitidas(IEnumerable<RetornoNotaFiscal> resultadoConsulta)
            {
                ResultadoConsulta = resultadoConsulta;
            }

            public IEnumerable<RetornoNotaFiscal> ResultadoConsulta { get; }
        }

        public class ResultadoNotasTransmitidas // mudar para Success ou Failure (Result.Ok, Result.Error)
        {
            public ResultadoNotasTransmitidas(List<string> erros)
            {
                Erros = erros;
            }

            public List<string> Erros { get; }
        }

        #endregion Messages

        private const string MensagemErro = "Tentativa de transmissão de notas em contingência falhou. Serviço continua indisponível.";
        private const string NFE_NAMESPACE = "http://www.portalfiscal.inf.br/nfe";
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IEmitenteRepository _emissorService;
        private readonly IConsultarNotaFiscalService nfeConsulta;
        private readonly IServiceFactory _serviceFactory;
        private readonly CertificadoService certificadoService;
        private readonly SefazSettings _sefazSettings;
        private readonly IMapper mapper;
        private readonly INotaFiscalRepository _notaFiscalRepository;

        public EmiteNFeContingenciaActor(INotaFiscalRepository notaFiscalRepository, IEmitenteRepository emissorService,
            IConsultarNotaFiscalService nfeConsulta, IServiceFactory serviceFactory, CertificadoService certificadoService,
            SefazSettings sefazSettings, IMapper mapper)
        {
            _notaFiscalRepository = notaFiscalRepository;
            _emissorService = emissorService;
            this.nfeConsulta = nfeConsulta;
            _serviceFactory = serviceFactory;
            this.certificadoService = certificadoService;
            _sefazSettings = sefazSettings;
            this.mapper = mapper;

            /* Actors para criar:
             * NFeConsulta
             * NFeInutilizada
             * CancelarNFe
             */

            ReceiveAsync<TransmitirNFeEmContingencia>(HandleTransmitirNFeEmContingenciaAsync);
            Receive<TransmiteNFes>(HandleTransmiteNFes);
            ReceiveAsync<ConsultaRecibos>(HandleConsultaRecibosAsync);
            Receive<ValidaNFesTransmitidas>(HandleValidaNFesTransmitidas);
        }

        private async Task HandleTransmitirNFeEmContingenciaAsync(TransmitirNFeEmContingencia msg)
        {
            log.Info("Mensagem para transmitir notas emitidas em contingência recebida.");

            var notas = _notaFiscalRepository.GetNotasContingencia();

            var notasNFe = new List<string>();
            var notasNfCe = new List<string>();

            foreach (var nota in notas)
            {
                var xml = await nota.LoadXmlAsync();

                if (nota.Modelo.Equals("55"))
                    notasNFe.Add(xml);
                else
                    notasNfCe.Add(xml);
            }

            if (notasNfCe.Count != 0)
                Self.Tell(new TransmiteNFes(notasNfCe, Modelo.Modelo65), Sender);

            if (notasNFe.Count != 0)
                Self.Tell(new TransmiteNFes(notasNFe, Modelo.Modelo55), Sender);
        }

        private void HandleTransmiteNFes(TransmiteNFes msg)
        {
            var retornoTransmissao = TransmitirLoteNotasFiscaisContingencia(msg.Nfe, msg.Modelo);

            switch (retornoTransmissao.TipoMensagem)
            {
                case TipoMensagem.ErroValidacao:
                    Sender.Tell(new ResultadoNotasTransmitidas(new List<string> { retornoTransmissao.Mensagem }));
                    log.Error("Erro ao receber retorno da transmissão de lote em contingência: " + retornoTransmissao.Mensagem);
                    return;

                case TipoMensagem.ServicoIndisponivel:
                    Sender.Tell(new ResultadoNotasTransmitidas(new List<string> { MensagemErro }));
                    log.Error("Erro ao receber retorno da transmissão de lote em contingência: " + MensagemErro);
                    return;
            }

            var tempoEspera = int.Parse(retornoTransmissao.RetEnviNFeInfRec.tMed) * 1000;

            Self.Tell(new ConsultaRecibos(tempoEspera, retornoTransmissao, msg.Modelo), Sender);
        }

        private async Task HandleConsultaRecibosAsync(ConsultaRecibos msg)
        {
            await Task.Delay(msg.TempoEspera);

            var resultadoConsulta = ConsultarReciboLoteContingencia(msg.MensagemRetorno.RetEnviNFeInfRec.nRec, msg.Modelo);
            Self.Tell(new ValidaNFesTransmitidas(resultadoConsulta), Sender);
        }

        private void HandleValidaNFesTransmitidas(ValidaNFesTransmitidas msg)
        {
            if (msg.ResultadoConsulta == null)
                Sender.Tell(new Akka.Actor.Status.Failure(new ConsultaReciboException("Resultado da consulta nulo.")));

            var erros = new List<string>();

            msg.ResultadoConsulta.ToList().ForEach(async resultado =>
            {
                var nota = _notaFiscalRepository.GetNotaFiscalByChave(resultado.Chave);

                if (resultado.CodigoStatus == "100")
                {
                    (NotaFiscalEntity, string) dadosPreenchidos = await PreencheDadosNotaFiscalAposEnvio(resultado, nota);
                    _notaFiscalRepository.Salvar(dadosPreenchidos.Item1, dadosPreenchidos.Item2);
                }
                else
                {
                    if (IsNotaDuplicada(resultado))
                        await TentaCorrigirNotaDuplicada(erros, resultado, nota);
                    else
                        erros.Add($"Modelo: {nota.Modelo} Nota: {nota.Numero} Série: {nota.Serie} \nMotivo: {resultado.Motivo}"); //O que fazer com essas mensagens de erro?
                }
            });

            Sender.Tell(new ResultadoNotasTransmitidas(erros));
        }

        protected virtual async Task<(NotaFiscalEntity, string)> PreencheDadosNotaFiscalAposEnvio(RetornoNotaFiscal resultado, NotaFiscalEntity nota)
        {
            nota.DataAutorizacao = DateTime.ParseExact(resultado.DataAutorizacao, "yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture);
            nota.Protocolo = resultado.Protocolo.Numero;
            nota.Status = (int)Status.ENVIADA;

            var xml = await LoadXmlAsync(nota, resultado.Protocolo);

            return (nota, xml);
        }

        private static bool IsNotaDuplicada(RetornoNotaFiscal resultado)
        {
            return resultado.Motivo.Contains("Duplicidade");
        }

        protected virtual async Task TentaCorrigirNotaDuplicada(List<string> erros, RetornoNotaFiscal resultado, NotaFiscalEntity nota)
        {
            X509Certificate2 certificado = certificadoService.GetX509Certificate2();
            Emissor emitente = _emissorService.GetEmissor();

            RetornoConsulta retornoConsulta = nfeConsulta.ConsultarNotaFiscal(
                nota.Chave, emitente.Endereco.CodigoUF, certificado,
                nota.Modelo.Equals("65") ? Modelo.Modelo65 : Modelo.Modelo55);

            if (retornoConsulta.IsEnviada)
            {
                nota.DataAutorizacao = retornoConsulta.DhAutorizacao;
                nota.Protocolo = retornoConsulta.Protocolo.Numero;
                nota.Status = (int)Status.ENVIADA;

                string xml = await LoadXmlAsync(nota, retornoConsulta.Protocolo);
                _notaFiscalRepository.Salvar(nota, xml);
            }
            else
            {
                erros.Add($"Modelo: {nota.Modelo} Nota: {nota.Numero} Série: {nota.Serie} \nMotivo: {resultado.Motivo}"); //O que fazer com essas mensagens de erro?
            }
        }

        protected virtual async Task<string> LoadXmlAsync(NotaFiscalEntity nota, Protocolo protocolo)
        {
            var xml = await nota.LoadXmlAsync();
            xml = xml.Replace("<protNFe />", protocolo.Xml);
            return xml;
        }

        protected virtual MensagemRetornoTransmissaoNotasContingencia TransmitirLoteNotasFiscaisContingencia(List<string> nfeList, Modelo modelo)
        {
            var lote = new TEnviNFe
            {
                idLote = "999999",
                indSinc = TEnviNFeIndSinc.Item0,
                versao = "4.00",
                NFe = new TNFe[1]
            };
            //qual a regra pra gerar o id?
            //apenas uma nota no lote
            lote.NFe[0] = new TNFe(); //Gera tag <NFe /> vazia para usar no replace

            string parametroXml =
                XmlUtil.Serialize(lote, NFE_NAMESPACE)
                    .Replace("<NFe />", GerarXmlListaNFe(nfeList))
                    .Replace("<motDesICMS>1</motDesICMS>", string.Empty);

            var document = new XmlDocument();
            document.LoadXml(parametroXml);
            var node = document.DocumentElement;

            var codigoUf = (CodigoUfIbge)Enum.Parse(typeof(CodigoUfIbge), _emissorService.GetEmissor().Endereco.UF);
            var certificado = certificadoService.GetX509Certificate2();
            var servico = _serviceFactory.GetService(modelo, Servico.AUTORIZACAO, codigoUf, certificado);
            var client = (NFeAutorizacao4SoapClient)servico.SoapClient;
            var result = client.nfeAutorizacaoLote(node);
            var retorno = (TRetEnviNFe)XmlUtil.Deserialize<TRetEnviNFe>(result.OuterXml);

            return new MensagemRetornoTransmissaoNotasContingencia
            {
                RetEnviNFeInfRec = (TRetEnviNFeInfRec)retorno.Item,
                TipoMensagem = TipoMensagem.Sucesso
            };
        }

        protected virtual IEnumerable<RetornoNotaFiscal> ConsultarReciboLoteContingencia(string nRec, Modelo modelo)
        {
            var recibo = new TConsReciNFe
            {
                versao = "4.00",
                tpAmb = _sefazSettings.Ambiente == Ambiente.Producao ?
                    RetAutorizacao.Envio.TAmb.Item1 : RetAutorizacao.Envio.TAmb.Item2,
                nRec = nRec
            };

            var node = new XmlDocument();
            node.LoadXml(XmlUtil.Serialize(recibo, NFE_NAMESPACE));

            var codigoUf = (CodigoUfIbge)Enum.Parse(typeof(CodigoUfIbge), _emissorService.GetEmissor().Endereco.UF);
            X509Certificate2 certificado = certificadoService.GetX509Certificate2();
            var servico = _serviceFactory.GetService(modelo, Servico.RetAutorizacao, codigoUf, certificado);
            var client = servico.SoapClient as NFeRetAutorizacao4SoapClient;
            var result = client.nfeRetAutorizacaoLote(node);
            var retorno = XmlUtil.Deserialize<TRetConsReciNFe>(result.OuterXml) as TRetConsReciNFe;

            return retorno.protNFe.Select(protNFe => new RetornoNotaFiscal
            {
                Chave = protNFe.infProt.chNFe,
                CodigoStatus = protNFe.infProt.cStat,
                DataAutorizacao = protNFe.infProt.dhRecbto,
                Motivo = protNFe.infProt.xMotivo,
                Protocolo = new Protocolo(mapper.Map<Consulta.TProtNFe>(protNFe)),
                Xml = XmlUtil.Serialize(protNFe, string.Empty)
                        .Replace("<?xml version=\"1.0\" encoding=\"utf-8\"?>", string.Empty)
                        .Replace("TProtNFe", "protNFe")
                        .Replace("<infProt xmlns=\"http://www.portalfiscal.inf.br/nfe\">", "<infProt>")
            });
        }

        public static string GerarXmlListaNFe(IEnumerable<string> notasFiscais)
        {
            var notasConcatenadas = new StringBuilder();

            foreach (string nota in notasFiscais)
            {
                var nfeProc = new XmlDocument();
                nfeProc.LoadXml(nota);
                notasConcatenadas.Append(nfeProc.GetElementsByTagName("NFe")[0].OuterXml);
            }

            return notasConcatenadas.ToString();
        }
    }
}