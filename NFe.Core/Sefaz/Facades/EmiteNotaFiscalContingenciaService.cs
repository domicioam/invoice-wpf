using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using NFe.Core.Cadastro.Certificado;
using NFe.Core.Cadastro.Configuracoes;
using NFe.Core.Cadastro.Emissor;
using NFe.Core.Entitities;
using NFe.Core.Interfaces;
using NFe.Core.NFeAutorizacao4;
using NFe.Core.NFeRetAutorizacao4;
using NFe.Core.NotasFiscais.Sefaz.NfeAutorizacao;
using NFe.Core.NotasFiscais.Sefaz.NfeConsulta2;
using NFe.Core.Sefaz.Facades;
using NFe.Core.Utils;
using NFe.Core.Utils.Assinatura;
using NFe.Core.Utils.Conversores;
using NFe.Core.Utils.QrCode;
using NFe.Core.Utils.Xml;
using NFe.Core.XmlSchemas.NfeAutorizacao.Envio;
using NFe.Core.XmlSchemas.NfeAutorizacao.Retorno;
using NFe.Core.XmlSchemas.NfeRetAutorizacao.Envio;
using NFe.Core.XmlSchemas.NfeRetAutorizacao.Retorno;

namespace NFe.Core.NotasFiscais.Services
{
    public interface IEmiteNotaFiscalContingenciaFacade
    {
        int EmitirNotaContingencia(NotaFiscal notaFiscal, string cscId, string csc);
        Task<List<string>> TransmitirNotasFiscalEmContingencia();
        void InutilizarCancelarNotasPendentesContingencia(NotaFiscalEntity notaParaCancelar, INotaFiscalRepository notaFiscalRepository);
    }

    public class EmiteEmiteNotaFiscalContingenciaFacade : IEmiteNotaFiscalContingenciaFacade
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const string MensagemErro =
            "Tentativa de transmissão de notas em contingência falhou. Serviço continua indisponível.";

        private readonly IConfiguracaoService _configuracaoService;
        private readonly ICertificadoRepository _certificadoRepository;
        private readonly ICertificateManager _certificateManager;
        private readonly INotaFiscalRepository _notaFiscalRepository;

        private bool _isFirstTimeRecheckingRecipts;
        private bool _isFirstTimeResending;
        private readonly IEmissorService _emissorService;
        private readonly INFeConsulta _nfeConsulta;
        private readonly IServiceFactory _serviceFactory;
        private readonly ICertificadoService _certificadoService;
        private readonly InutilizarNotaFiscalFacade _notaInutilizadaFacade;
        private readonly ICancelaNotaFiscalFacade _cancelaNotaFiscalService;

        public EmiteEmiteNotaFiscalContingenciaFacade(IConfiguracaoService configuracaoService, ICertificadoRepository certificadoRepository, ICertificateManager certificateManager, INotaFiscalRepository notaFiscalRepository,  IEmissorService emissorService, INFeConsulta nfeConsulta, IServiceFactory serviceFactory, ICertificadoService certificadoService, InutilizarNotaFiscalFacade notaInutilizadaFacade, ICancelaNotaFiscalFacade cancelaNotaFiscalService)
        {
            _configuracaoService = configuracaoService;
            _certificadoRepository = certificadoRepository;
            _certificateManager = certificateManager;
            _notaFiscalRepository = notaFiscalRepository;
            _emissorService = emissorService;
            _nfeConsulta = nfeConsulta;
            _serviceFactory = serviceFactory;
            _certificadoService = certificadoService;
            _notaInutilizadaFacade = notaInutilizadaFacade;
            _cancelaNotaFiscalService = cancelaNotaFiscalService;
        }

        public int EmitirNotaContingencia(NotaFiscal notaFiscal, string cscId, string csc)
        {
            var qrCode = string.Empty;
            string newNodeXml;
            const string nFeNamespaceName = "http://www.portalfiscal.inf.br/nfe";
            var digVal = string.Empty;

            var config = _configuracaoService.GetConfiguracao();

            notaFiscal.Identificacao.Numero = _configuracaoService.ObterProximoNumeroNotaFiscal(notaFiscal.Identificacao.Modelo);
            notaFiscal.Identificacao.DataHoraEntradaContigencia = config.DataHoraEntradaContingencia;
            notaFiscal.Identificacao.JustificativaContigencia = config.JustificativaContingencia;
            notaFiscal.Identificacao.TipoEmissao = notaFiscal.Identificacao.Modelo == Modelo.Modelo65
                ? TipoEmissao.ContigenciaNfce
                : TipoEmissao.FsDa;
            notaFiscal.CalcularChave();

            X509Certificate2 certificado;

            var certificadoEntity = _certificadoRepository.GetCertificado();

            if (!string.IsNullOrWhiteSpace(certificadoEntity.Caminho))
                certificado = _certificateManager.GetCertificateByPath(certificadoEntity.Caminho,
                    RijndaelManagedEncryption.DecryptRijndael(certificadoEntity.Senha));
            else
                certificado = _certificateManager.GetCertificateBySerialNumber(certificadoEntity.NumeroSerial, false);

            if (notaFiscal.Identificacao.Ambiente == Ambiente.Homologacao)
                notaFiscal.Produtos[0].Descricao = "NOTA FISCAL EMITIDA EM AMBIENTE DE HOMOLOGACAO - SEM VALOR FISCAL";

            var refUri = "#NFe" + notaFiscal.Identificacao.Chave;

            var xml = Regex.Replace(XmlUtil.GerarXmlLoteNFe(notaFiscal, nFeNamespaceName), "<motDesICMS>1</motDesICMS>",
                string.Empty);
            XmlNode node = AssinaturaDigital.AssinarLoteComUmaNota(xml, refUri, certificado, ref digVal);

            if (notaFiscal.Identificacao.Modelo == Modelo.Modelo65)
            {
                qrCode = QrCodeUtil.GerarQrCodeNFe(notaFiscal.Identificacao.Chave, notaFiscal.Destinatario,
                    digVal, notaFiscal.Identificacao.Ambiente,
                    notaFiscal.Identificacao.DataHoraEmissao,
                    notaFiscal.TotalNFe.IcmsTotal.ValorTotalNFe.ToString("F", CultureInfo.InvariantCulture),
                    notaFiscal.TotalNFe.IcmsTotal.ValorTotalIcms.ToString("F", CultureInfo.InvariantCulture), cscId,
                    csc, notaFiscal.Identificacao.TipoEmissao);

                newNodeXml = node.InnerXml.Replace("<qrCode />", "<qrCode>" + qrCode + "</qrCode>");
            }
            else
            {
                newNodeXml = node.InnerXml.Replace("<infNFeSupl><qrCode /></infNFeSupl>", "");
            }

            var document = new XmlDocument();
            document.LoadXml(newNodeXml);
            node = document.DocumentElement;

            if (node == null) throw new ArgumentException("Xml inválido.");

            var lote = (TEnviNFe)XmlUtil.Deserialize<TEnviNFe>(node.OuterXml);
            var nfe = lote.NFe[0];

            //salvar nota PreEnvio aqui
            notaFiscal.Identificacao.Status = Status.CONTINGENCIA;

            var idNotaCopiaSeguranca =  _notaFiscalRepository.SalvarNotaFiscalPendente(notaFiscal,
                XmlUtil.GerarNfeProcXml(nfe, qrCode),
                notaFiscal.Identificacao.Ambiente);

            var notaFiscalEntity =  _notaFiscalRepository.GetNotaFiscalById(idNotaCopiaSeguranca, false);
            notaFiscalEntity.Status = (int)Status.CONTINGENCIA;
            var nfeProcXml = XmlUtil.GerarNfeProcXml(nfe, qrCode);

             _notaFiscalRepository.Salvar(notaFiscalEntity, nfeProcXml);
            notaFiscal.QrCodeUrl = qrCode;
            return idNotaCopiaSeguranca;
        }

        public async Task<List<string>> TransmitirNotasFiscalEmContingencia() //Chamar esse método quando o serviço voltar
        {
            var erros = new List<string>();

            var notas = _notaFiscalRepository.GetNotasContingencia();

            var config = _configuracaoService.GetConfiguracao();
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

            try
            {
                if (notasNfCe.Count() != 0)
                    erros = await TransmitirConsultarLoteContingenciaAsync(config, notasNfCe, Modelo.Modelo65);

                if (notasNFe.Count() != 0)
                    erros = await TransmitirConsultarLoteContingenciaAsync(config, notasNFe, Modelo.Modelo55);
            }
            catch (Exception e)
            {
                log.Error(e);
                var sDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "EmissorNFeDir");

                if (!Directory.Exists(sDirectory)) Directory.CreateDirectory(sDirectory);

                using (var stream = File.Create(Path.Combine(sDirectory, "logTransmitirContingencia.txt")))
                {
                    using (var writer = new StreamWriter(stream))
                    {
                        writer.WriteLine(e.ToString());
                    }
                }

                return null;
            }

            return erros;
        }

        public void InutilizarCancelarNotasPendentesContingencia(NotaFiscalEntity notaParaCancelar,
            INotaFiscalRepository notaFiscalRepository)
        {
            if (notaParaCancelar == null || notaParaCancelar.Status == 0)
                return;

            var emitente = _emissorService.GetEmissor();
            var ufEmissor = emitente.Endereco.UF;
            var codigoUf = UfToCodigoUfConversor.GetCodigoUf(ufEmissor);

            var certificado = _certificadoService.GetX509Certificate2();
            var ambiente = (Ambiente)notaParaCancelar.Ambiente - 1;
            var modelo = notaParaCancelar.Modelo.Equals("55") ? Modelo.Modelo55 : Modelo.Modelo65;

            var result =
                _nfeConsulta.ConsultarNotaFiscal(notaParaCancelar.Chave, codigoUf, certificado, ambiente, modelo);
            var codigoUfEnum = (CodigoUfIbge)Enum.Parse(typeof(CodigoUfIbge), emitente.Endereco.UF);

            if (result.IsEnviada)
            {
                _cancelaNotaFiscalService.CancelarNotaFiscal(ufEmissor, codigoUfEnum, ambiente, emitente.CNPJ,
                    notaParaCancelar.Chave, result.Protocolo.infProt.nProt, modelo, "Nota duplicada em contingência");
            }
            else
            {
                var resultadoInutilizacao = _notaInutilizadaFacade.InutilizarNotaFiscal(ufEmissor, codigoUfEnum,
                    ambiente, emitente.CNPJ, modelo, notaParaCancelar.Serie,
                    notaParaCancelar.Numero, notaParaCancelar.Numero);

                if (resultadoInutilizacao.Status != Sefaz.NfeInutilizacao2.Status.ERRO)
                    _notaFiscalRepository.ExcluirNota(notaParaCancelar.Chave, ambiente);
            }
        }

        private async Task<List<string>> TransmitirConsultarLoteContingenciaAsync(ConfiguracaoEntity config,
    List<string> notasNfCe, Modelo modelo)
        {
            var retornoTransmissao = TransmitirLoteNotasFiscaisContingencia(notasNfCe, modelo);

            switch (retornoTransmissao.TipoMensagem)
            {
                case TipoMensagem.ErroValidacao:
                    return new List<string> { retornoTransmissao.Mensagem };
                case TipoMensagem.ServicoIndisponivel:
                    return new List<string> { MensagemErro };
            }

            var tempoEspera = int.Parse(retornoTransmissao.RetEnviNFeInfRec.tMed) * 1000;
            var erros = new List<string>();
            Thread.Sleep(tempoEspera);
            var resultadoConsulta = ConsultarReciboLoteContingencia(retornoTransmissao.RetEnviNFeInfRec.nRec, modelo);

            if (resultadoConsulta == null) return new List<string> { MensagemErro };

            foreach (var resultado in resultadoConsulta)
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
                        X509Certificate2 certificado;
                        var certificadoEntity = _certificadoRepository.GetCertificado();
                        var emitente = _emissorService.GetEmissor();

                        if (!string.IsNullOrWhiteSpace(certificadoEntity.Caminho))
                            certificado = _certificateManager.GetCertificateByPath(certificadoEntity.Caminho,
                                RijndaelManagedEncryption.DecryptRijndael(certificadoEntity.Senha));
                        else
                            certificado =
                                _certificateManager.GetCertificateBySerialNumber(certificadoEntity.NumeroSerial, false);

                        var retornoConsulta = _nfeConsulta.ConsultarNotaFiscal
                        (
                            nota.Chave,
                            emitente.Endereco.CodigoUF,
                            certificado,
                            config.IsProducao ? Ambiente.Producao : Ambiente.Homologacao,
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

            return erros;
        }

        private List<RetornoNotaFiscal> ConsultarReciboLoteContingencia(string nRec, Modelo modelo)
        {
            var config = _configuracaoService.GetConfiguracao();
            X509Certificate2 certificado;

            var certificadoEntity = _certificadoRepository.GetCertificado();

            if (!string.IsNullOrWhiteSpace(certificadoEntity.Caminho))
                certificado = _certificateManager.GetCertificateByPath(certificadoEntity.Caminho,
                    RijndaelManagedEncryption.DecryptRijndael(certificadoEntity.Senha));
            else
                certificado = _certificateManager.GetCertificateBySerialNumber(certificadoEntity.NumeroSerial, false);

            var consultaRecibo = new TConsReciNFe
            {
                versao = "4.00",
                tpAmb = config.IsProducao ? XmlSchemas.NfeRetAutorizacao.Envio.TAmb.Item1 : XmlSchemas.NfeRetAutorizacao.Envio.TAmb.Item2,
                nRec = nRec
            };

            var parametroXml = XmlUtil.Serialize(consultaRecibo, "http://www.portalfiscal.inf.br/nfe");

            var node = new XmlDocument();
            node.LoadXml(parametroXml);

            var ambiente = config.IsProducao ? Ambiente.Producao : Ambiente.Homologacao;
            var codigoUf = (CodigoUfIbge)Enum.Parse(typeof(CodigoUfIbge), _emissorService.GetEmissor().Endereco.UF);

            try
            {
                var servico =
                    _serviceFactory.GetService(modelo, ambiente, Servico.RetAutorizacao, codigoUf, certificado);
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
                })
                    .ToList();
            }
            catch (Exception e)
            {
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

        private MensagemRetornoTransmissaoNotasContingencia TransmitirLoteNotasFiscaisContingencia(List<string> nfeList,
    Modelo modelo)
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
            parametroXml = parametroXml.Replace("<NFe />", XmlUtil.GerarXmlListaNFe(nfeList))
                .Replace("<motDesICMS>1</motDesICMS>", string.Empty);

            var document = new XmlDocument();
            document.LoadXml(parametroXml);
            var node = document.DocumentElement;

            var config = _configuracaoService.GetConfiguracao();

            var ambiente = config.IsProducao ? Ambiente.Producao : Ambiente.Homologacao;
            var codigoUf = (CodigoUfIbge)Enum.Parse(typeof(CodigoUfIbge), _emissorService.GetEmissor().Endereco.UF);
            X509Certificate2 certificado;

            var certificadoEntity = _certificadoRepository.GetCertificado();

            if (!string.IsNullOrWhiteSpace(certificadoEntity.Caminho))
                certificado = _certificateManager.GetCertificateByPath(certificadoEntity.Caminho,
                    RijndaelManagedEncryption.DecryptRijndael(certificadoEntity.Senha));
            else
                certificado = _certificateManager.GetCertificateBySerialNumber(certificadoEntity.NumeroSerial, false);

            try
            {
                var servico = _serviceFactory.GetService(modelo, ambiente, Servico.AUTORIZACAO, codigoUf, certificado);
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
                log.Error(e);
                if (!_isFirstTimeResending)
                {
                    _isFirstTimeResending = true;
                    return TransmitirLoteNotasFiscaisContingencia(nfeList, modelo);
                }

                _isFirstTimeResending = false;

                return new MensagemRetornoTransmissaoNotasContingencia
                {
                    TipoMensagem = TipoMensagem.ServicoIndisponivel
                };
            }
        }
    }
}