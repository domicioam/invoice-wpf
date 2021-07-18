using Akka.Actor;
using NFe.Core;
using NFe.Core.Cadastro.Certificado;
using NFe.Core.Domain;
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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Status = NFe.Core.Entitities.Status;

namespace DgSystems.NFe.Services.Actors
{
    public class EmiteNFeContingenciaActor : ReceiveActor
    {
        #region Messages
        public class TransmitirNFeEmContingencia { }
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

            public ValidaNFesTransmitidas(List<RetornoNotaFiscal> resultadoConsulta)
            {
                ResultadoConsulta = resultadoConsulta;
            }

            public List<RetornoNotaFiscal> ResultadoConsulta { get; }
        }

        public class ResultadoNotasTransmitidas // mudar para Success ou Failure (Result.Ok, Result.Error)
        {
            public ResultadoNotasTransmitidas(List<string> erros)
            {
                Erros = erros;
            }

            public List<string> Erros { get; }
        }
        #endregion

        private const string MensagemErro = "Tentativa de transmissão de notas em contingência falhou. Serviço continua indisponível.";
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private bool _isFirstTimeRecheckingRecipts;
        private bool _isFirstTimeResending;

        private readonly IEmitenteRepository _emissorService;
        private readonly IConsultarNotaFiscalService _nfeConsulta;
        private readonly IServiceFactory _serviceFactory;
        private readonly ICertificadoService _certificadoService;
        private readonly SefazSettings _sefazSettings;
        private readonly INotaFiscalRepository _notaFiscalRepository;

        public EmiteNFeContingenciaActor(INotaFiscalRepository notaFiscalRepository, IEmitenteRepository emissorService, IConsultarNotaFiscalService nfeConsulta, IServiceFactory serviceFactory, ICertificadoService certificadoService, SefazSettings sefazSettings)
        {
            _notaFiscalRepository = notaFiscalRepository;
            _emissorService = emissorService;
            _nfeConsulta = nfeConsulta;
            _serviceFactory = serviceFactory;
            _certificadoService = certificadoService;
            _sefazSettings = sefazSettings;

            /* Actors para criar:
             * NFeConsulta
             * NFeInutilizada
             * CancelarNFe
             */

            ReceiveAsync<TransmitirNFeEmContingencia>(HandleTransmitirNFeEmContingenciaAsync);
            Receive<TransmiteNFes>(HandleTransmiteNFes);
            ReceiveAsync<ConsultaRecibos>(HandleConsultaRecibosAsync);
            ReceiveAsync<ValidaNFesTransmitidas>(HandleValidaNFesTransmitidas);
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

            if(notasNfCe.Count != 0)
                Self.Tell(new TransmiteNFes(notasNfCe, Modelo.Modelo65), Sender);

            if(notasNFe.Count != 0)
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

            Self.Tell(new ConsultaRecibos(tempoEspera, retornoTransmissao, msg.Modelo));
        }

        private async Task HandleConsultaRecibosAsync(ConsultaRecibos msg)
        {
            await Task.Delay(msg.TempoEspera);

            var resultadoConsulta = ConsultarReciboLoteContingencia(msg.MensagemRetorno.RetEnviNFeInfRec.nRec, msg.Modelo);
            Self.Tell(new ValidaNFesTransmitidas(resultadoConsulta));
        }

        private async Task HandleValidaNFesTransmitidas(ValidaNFesTransmitidas msg)
        {
            var erros = new List<string>();

            if (msg.ResultadoConsulta == null)
                Sender.Tell(new Akka.Actor.Status.Failure(new ConsultaReciboException("Resultado da consulta nulo.")));

            foreach (var resultado in msg.ResultadoConsulta)
            {
                var nota = _notaFiscalRepository.GetNotaFiscalByChave(resultado.Chave);

                if (resultado.CodigoStatus == "100")
                {
                    nota.DataAutorizacao = DateTime.ParseExact(resultado.DataAutorizacao, "yyyy-MM-ddTHH:mm:sszzz",
                        CultureInfo.InvariantCulture);
                    nota.Protocolo = resultado.Protocolo;
                    nota.Status = (int)Status.ENVIADA;

                    var xml = await nota.LoadXmlAsync();
                    xml = xml.Replace("<protNFe />", resultado.Xml);

                    _notaFiscalRepository.Salvar(nota, xml);
                }
                else
                {
                    if (resultado.Motivo.Contains("Duplicidade"))
                    {
                        X509Certificate2 certificado = _certificadoService.GetX509Certificate2();
                        var emitente = _emissorService.GetEmissor();

                        var retornoConsulta = _nfeConsulta.ConsultarNotaFiscal
                        (
                            nota.Chave,
                            emitente.Endereco.CodigoUF,
                            certificado,
                            nota.Modelo.Equals("65") ? Modelo.Modelo65 : Modelo.Modelo55
                        );

                        if (retornoConsulta.IsEnviada)
                        {
                            var protSerialized = XmlUtil.Serialize(retornoConsulta.Protocolo, string.Empty)
                                .Replace("<?xml version=\"1.0\" encoding=\"utf-8\"?>", string.Empty)
                                .Replace("TProtNFe", "protNFe");

                            protSerialized = Regex.Replace(protSerialized, "<infProt (.*?)>", "<infProt>");

                            nota.DataAutorizacao = retornoConsulta.DhAutorizacao;
                            nota.Protocolo = retornoConsulta.Protocolo.infProt.nProt;
                            nota.Status = (int)Status.ENVIADA;

                            var xml = await nota.LoadXmlAsync();
                            xml = xml.Replace("<protNFe />", protSerialized);

                            _notaFiscalRepository.Salvar(nota, xml);
                        }
                        else
                        {
                            erros.Add(
                                $"Modelo: {nota.Modelo} Nota: {nota.Numero} Série: {nota.Serie} \nMotivo: {resultado.Motivo}"); //O que fazer com essas mensagens de erro?
                        }
                    }
                    else
                    {
                        erros.Add(
                            $"Modelo: {nota.Modelo} Nota: {nota.Numero} Série: {nota.Serie} \nMotivo: {resultado.Motivo}"); //O que fazer com essas mensagens de erro?
                    }
                }
            }

            Sender.Tell(new ResultadoNotasTransmitidas(erros));
        }

        public virtual MensagemRetornoTransmissaoNotasContingencia TransmitirLoteNotasFiscaisContingencia(List<string> nfeList, Modelo modelo)
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

            var parametroXml = XmlUtil.Serialize(lote, "http://www.portalfiscal.inf.br/nfe");
            parametroXml = parametroXml.Replace("<NFe />", GerarXmlListaNFe(nfeList))
                .Replace("<motDesICMS>1</motDesICMS>", string.Empty);

            var document = new XmlDocument();
            document.LoadXml(parametroXml);
            var node = document.DocumentElement;

            var codigoUf = (CodigoUfIbge)Enum.Parse(typeof(CodigoUfIbge), _emissorService.GetEmissor().Endereco.UF);
            X509Certificate2 certificado = _certificadoService.GetX509Certificate2();

            try
            {
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
            catch (Exception e)
            {
                log.Error("Erro ao tentar acessar o serviço de autorização ao transmitir notas fiscais emitidas em contingência.", e);
                if (!_isFirstTimeResending)
                {
                    _isFirstTimeResending = true;


                    //TODO: Refactor to use states to avoid if else


                    return TransmitirLoteNotasFiscaisContingencia(nfeList, modelo);
                }

                _isFirstTimeResending = false;

                return new MensagemRetornoTransmissaoNotasContingencia
                {
                    TipoMensagem = TipoMensagem.ServicoIndisponivel
                };
            }
        }

        private List<RetornoNotaFiscal> ConsultarReciboLoteContingencia(string nRec, Modelo modelo)
        {
            X509Certificate2 certificado = _certificadoService.GetX509Certificate2();

            var consultaRecibo = new TConsReciNFe
            {
                versao = "4.00",
                tpAmb = _sefazSettings.Ambiente == Ambiente.Producao ? global::NFe.Core.XmlSchemas.NfeRetAutorizacao.Envio.TAmb.Item1 : global::NFe.Core.XmlSchemas.NfeRetAutorizacao.Envio.TAmb.Item2,
                nRec = nRec
            };

            var parametroXml = XmlUtil.Serialize(consultaRecibo, "http://www.portalfiscal.inf.br/nfe");

            var node = new XmlDocument();
            node.LoadXml(parametroXml);

            var codigoUf = (CodigoUfIbge)Enum.Parse(typeof(CodigoUfIbge), _emissorService.GetEmissor().Endereco.UF);

            try
            {
                var servico = _serviceFactory.GetService(modelo, Servico.RetAutorizacao, codigoUf, certificado);
                var client = (NFeRetAutorizacao4SoapClient)servico.SoapClient;

                var result = client.nfeRetAutorizacaoLote(node);

                var retorno = (TRetConsReciNFe)XmlUtil.Deserialize<TRetConsReciNFe>(result.OuterXml);

                return retorno.protNFe.Select(protNFe => new RetornoNotaFiscal
                {
                    Chave = protNFe.infProt.chNFe,
                    CodigoStatus = protNFe.infProt.cStat,
                    DataAutorizacao = protNFe.infProt.dhRecbto,
                    Motivo = protNFe.infProt.xMotivo,
                    Protocolo = protNFe.infProt.nProt,
                    Xml = XmlUtil.Serialize(protNFe, string.Empty)
                            .Replace("<?xml version=\"1.0\" encoding=\"utf-8\"?>", string.Empty)
                            .Replace("TProtNFe", "protNFe")
                            .Replace("<infProt xmlns=\"http://www.portalfiscal.inf.br/nfe\">", "<infProt>")
                }).ToList();
            }
            catch (Exception e)
            {

                //TODO: Refactor to use states to avoid if else


                log.Error(e);
                if (!_isFirstTimeRecheckingRecipts)
                {
                    _isFirstTimeRecheckingRecipts = true;
                    return ConsultarReciboLoteContingencia(nRec, modelo);
                }

                _isFirstTimeRecheckingRecipts = false;
                return null;
            }
        }

        public static string GerarXmlListaNFe(List<string> notasFiscais)
        {
            var notasConcatenadas = new StringBuilder();

            for (var i = 0; i < notasFiscais.Count; i++)
            {
                var nfeProc = new XmlDocument();
                nfeProc.LoadXml(notasFiscais[i]);
                notasConcatenadas.Append(nfeProc.GetElementsByTagName("NFe")[0].OuterXml);
            }

            return notasConcatenadas.ToString();
        }
    }
}
