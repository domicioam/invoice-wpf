using System;
using System.Xml;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Security.Cryptography.X509Certificates;

using NFe.Core.Sefaz;
using NFe.Core.Utils;
using NFe.Core.Utils.Xml;
using NFe.Core.Entitities;
using NFe.Core.Sefaz.Facades;
using NFe.Core.NFeAutorizacao4;
using NFe.Core.Utils.Assinatura;
using NFe.Core.Cadastro.Configuracoes;
using NFe.Core.NotasFiscais.Sefaz.NfeConsulta2;
using NFe.Core.XmlSchemas.NfeAutorizacao.Retorno;
using TProtNFe = NFe.Core.XmlSchemas.NfeAutorizacao.Retorno.TProtNFe;
using NFe.Core.Domain;
using NFe.Core.Interfaces;

namespace NFe.Core.NotasFiscais.Services
{
    public class EnviarNotaFiscalService : IEnviaNotaFiscalService
    {
        private const string DATE_STRING_FORMAT = "yyyy-MM-ddTHH:mm:sszzz";
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IConfiguracaoRepository _configuracaoService;
        private readonly IConsultarNotaFiscalService _nfeConsulta;
        private readonly IServiceFactory _serviceFactory;

        public EnviarNotaFiscalService(IConfiguracaoRepository configuracaoService, IServiceFactory serviceFactory, IConsultarNotaFiscalService nfeConsulta)
        {
            _configuracaoService = configuracaoService;
            _serviceFactory = serviceFactory;
            _nfeConsulta = nfeConsulta;
        }

        public ResultadoEnvio EnviarNotaFiscal(Domain.NotaFiscal notaFiscal, string cscId, string csc, X509Certificate2 certificado, XmlNFe xmlNFe)
        {
            if (!IsNotaFiscalValida(notaFiscal, cscId, csc, certificado))
            {
                throw new ArgumentException("Nota fiscal inválida.");
            }

            var nFeNamespaceName = "http://www.portalfiscal.inf.br/nfe";

            try
            {
                var codigoUf = (CodigoUfIbge)Enum.Parse(typeof(CodigoUfIbge), notaFiscal.Emitente.Endereco.UF);
                NFeAutorizacao4Soap client = CriarClientWS(notaFiscal, certificado, codigoUf);
                TProtNFe protocolo = InvocaServico_E_RetornaProtocolo(xmlNFe.XmlNode, client);

                if (IsSuccess(protocolo))
                {
                    notaFiscal = AtribuirValoresApósEnvioComSucesso(notaFiscal, xmlNFe.QrCode, protocolo);
                    var resultadoEnvio = new ResultadoEnvio(notaFiscal, protocolo, xmlNFe.QrCode, xmlNFe.TNFe, xmlNFe.XmlNode);
                    return resultadoEnvio;
                }
                else
                {
                    if (IsInvoiceDuplicated(protocolo))
                    {
                        var retornoConsulta = _nfeConsulta.ConsultarNotaFiscal(notaFiscal.Identificacao.Chave.ToString(), notaFiscal.Emitente.Endereco.CodigoUF, certificado, notaFiscal.Identificacao.Modelo);

                        var protSerialized = XmlUtil.Serialize(retornoConsulta.Protocolo, nFeNamespaceName);
                        var protDeserialized = (TProtNFe)XmlUtil.Deserialize<TProtNFe>(protSerialized);

                        notaFiscal = AtribuirValoresApósEnvioComSucesso(notaFiscal, xmlNFe.QrCode, protDeserialized);
                        var resultadoEnvio = new ResultadoEnvio(notaFiscal, protDeserialized, xmlNFe.QrCode, xmlNFe.TNFe, xmlNFe.XmlNode);
                        return resultadoEnvio;
                    }

                    //Nota continua com status pendente nesse caso
                    var mensagem = string.Concat("O xml informado é inválido de acordo com o validar da SEFAZ. Nota Fiscal não enviada!", "\n", protocolo.infProt.xMotivo);
                    throw new ArgumentException(mensagem);
                }
            }
            catch (Exception e)
            {
                log.Error(e);

                try
                {
                    var retornoConsulta = _nfeConsulta.ConsultarNotaFiscal(notaFiscal.Identificacao.Chave.ToString(), notaFiscal.Emitente.Endereco.CodigoUF, certificado, notaFiscal.Identificacao.Modelo);

                    if (retornoConsulta.IsEnviada)
                    {
                        var protSerialized = XmlUtil.Serialize(retornoConsulta.Protocolo, nFeNamespaceName);
                        var protDeserialized = (TProtNFe)XmlUtil.Deserialize<TProtNFe>(protSerialized);

                        notaFiscal = AtribuirValoresApósEnvioComSucesso(notaFiscal, xmlNFe.QrCode, protDeserialized);
                        var resultadoEnvio = new ResultadoEnvio(notaFiscal, protDeserialized, xmlNFe.QrCode, xmlNFe.TNFe, xmlNFe.XmlNode);
                        return resultadoEnvio;
                    }

                    throw;
                }
                catch (Exception retornoConsultaException)
                {
                    log.Error(retornoConsultaException);
                    throw;
                }
            }
            finally
            {
                _configuracaoService.SalvarPróximoNúmeroSérie(notaFiscal.Identificacao.Modelo,
                    notaFiscal.Identificacao.Ambiente);
            }
        }

        private static bool IsInvoiceDuplicated(TProtNFe protocolo)
        {
            return protocolo.infProt.xMotivo.Contains("Duplicidade");
        }

        private static bool IsSuccess(TProtNFe protocolo)
        {
            return protocolo.infProt.cStat.Equals("100");
        }

        private static TProtNFe InvocaServico_E_RetornaProtocolo(XmlNode node, NFeAutorizacao4Soap client)
        {
            var inValue = new nfeAutorizacaoLoteRequest { nfeDadosMsg = node };

            var result = client.nfeAutorizacaoLote(inValue).nfeResultMsg;
            var retorno = (TRetEnviNFe)XmlUtil.Deserialize<TRetEnviNFe>(result.OuterXml);
            var protocolo = (TProtNFe)retorno.Item;
            return protocolo;
        }

        private NFeAutorizacao4Soap CriarClientWS(Domain.NotaFiscal notaFiscal, X509Certificate2 certificado, CodigoUfIbge codigoUf)
        {
            var servico = _serviceFactory.GetService(notaFiscal.Identificacao.Modelo, Servico.AUTORIZACAO, codigoUf, certificado);
            var client = (NFeAutorizacao4Soap)servico.SoapClient;
            return client;
        }

        private static Domain.NotaFiscal AtribuirValoresApósEnvioComSucesso(Domain.NotaFiscal notaFiscal, QrCode qrCode, TProtNFe protocolo)
        {
            var dataAutorizacao = DateTime.ParseExact(protocolo.infProt.dhRecbto, DATE_STRING_FORMAT, CultureInfo.InvariantCulture);
            if (notaFiscal.Identificacao.Modelo == Modelo.Modelo65)
                notaFiscal.QrCodeUrl = qrCode.ToString();
            notaFiscal.Identificacao.Status = new StatusEnvio(Status.ENVIADA);
            notaFiscal.DhAutorizacao = dataAutorizacao.ToString("dd/MM/yyyy HH:mm:ss");
            notaFiscal.DataHoraAutorização = dataAutorizacao;
            notaFiscal.ProtocoloAutorizacao = protocolo.infProt.nProt;
            return notaFiscal;
        }

        public bool IsNotaFiscalValida(Domain.NotaFiscal notaFiscal, string cscId, string csc, X509Certificate2 certificado)
        {
            var refUri = "#NFe" + notaFiscal.Identificacao.Chave;
            var digVal = "";
            var nFeNamespaceName = "http://www.portalfiscal.inf.br/nfe";

            var xml = Regex.Replace(XmlUtil.GerarXmlLoteNFe(notaFiscal, nFeNamespaceName), "<motDesICMS>1</motDesICMS>",
                string.Empty);

            XmlNode node = AssinaturaDigital.AssinarLoteComUmaNota(xml, refUri, certificado, ref digVal);

            try
            {
                string newNodeXml;
                if (notaFiscal.Identificacao.Modelo == Modelo.Modelo65)
                {
                    QrCode qrCode = new QrCode();
                    qrCode.GerarQrCodeNFe(notaFiscal.Identificacao.Chave, notaFiscal.Destinatario, digVal,
                        notaFiscal.Identificacao.Ambiente,
                        notaFiscal.Identificacao.DataHoraEmissao,
                        notaFiscal.GetTotal().ToString("F", CultureInfo.InvariantCulture),
                        notaFiscal.GetTotalIcms().ToString("F", CultureInfo.InvariantCulture), cscId,
                        csc, notaFiscal.Identificacao.TipoEmissao);

                    newNodeXml = node.InnerXml.Replace("<qrCode />", "<qrCode>" + qrCode + "</qrCode>");
                }
                else
                {
                    newNodeXml = node.InnerXml;
                }

                var document = new XmlDocument();
                document.LoadXml(newNodeXml);
                node = document.DocumentElement;

                ValidadorXml.ValidarXml(node.OuterXml, "enviNFe_v4.00.xsd");

                return true;
            }
            catch (Exception e)
            {
                log.Error(e);
                return false;
            }
        }
    }
}