using NFe.Core.Cadastro.Certificado;
using NFe.Core.Cadastro.Configuracoes;
using NFe.Core.Domain;
using NFe.Core.Interfaces;
using NFe.Core.NfeStatusServico4;
using NFe.Core.Sefaz;
using NFe.Core.Utils.Conversores.Enums.StatusServico;
using NFe.Core.XMLSchemas.NfeStatusServico2.Envio;
using System;
using System.Configuration;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Xml;

namespace NFe.Core.NotasFiscais.Services
{
    public class ConsultaStatusServicoSefazService : IConsultaStatusServicoSefazService
    {
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const string SEFAZ_ENVIRONMENT = "production";
        private readonly ICertificadoRepository _certificadoRepository;
        private readonly CertificadoService _certificateManager;
        private readonly IEmitenteRepository _emissorService;

        public ConsultaStatusServicoSefazService(IEmitenteRepository emissorService, ICertificadoRepository certificadoService,
            CertificadoService certificateManager)
        {
            _emissorService = emissorService;
            _certificadoRepository = certificadoService;
            _certificateManager = certificateManager;
        }

        public bool ExecutarConsultaStatus(ConfiguracaoEntity config, Modelo modelo)
        {
            try
            {
                var sefazEnvironment = ConfigurationManager.AppSettings["sefazEnvironment"];

                var ambiente = sefazEnvironment == SEFAZ_ENVIRONMENT ? Ambiente.Producao : Ambiente.Homologacao;
                var codigoUf = (CodigoUfIbge)Enum.Parse(typeof(CodigoUfIbge), _emissorService.GetEmissor().Endereco.UF);
                var certificadoEntity = _certificadoRepository.GetCertificado();

                if (certificadoEntity == null)
                    return false;

                X509Certificate2 certificado = _certificateManager.GetX509Certificate2();
                return ConsultarStatus(codigoUf, ambiente, certificado, modelo);
            }catch(Exception e)
            {
                log.Error("Erro ao tentar consultar status do serviço da SEFAZ.", e);
                return false;
            }
        }

        private bool ConsultarStatus(CodigoUfIbge codigoUF, Ambiente ambiente, X509Certificate2 certificado, Modelo modelo)
        {
            try
            {
                var parametro = new TConsStatServ
                {
                    cUF = TCodUfIBGEConversor.ToTCodUfIBGE(codigoUF),
                    tpAmb = ambiente == Ambiente.Homologacao ? TAmb.Item2 : TAmb.Item1,
                    versao = "4.00",
                    xServ = TConsStatServXServ.STATUS
                };

                const string nFeNamespaceName = "http://www.portalfiscal.inf.br/nfe";
                string parametroXML = XmlUtil.Serialize(parametro, nFeNamespaceName);

                XmlDocument doc = new XmlDocument();
                XmlReader reader = XmlReader.Create(new StringReader(parametroXML));
                reader.MoveToContent();

                XmlNode node = doc.ReadNode(reader);
                string endpoint = "";

                if (modelo == Modelo.Modelo55)
                {
                    endpoint = "NfeStatusServico2";
                }
                else
                {
                    endpoint = "NfceStatusServico2";
                }

                var soapClient = new NfeStatusServico4SoapClient(endpoint);
                soapClient.ClientCredentials.ClientCertificate.Certificate = certificado;

                XmlNode result = soapClient.nfeStatusServicoNF(node);

                var retorno = (XmlSchemas.NfeStatusServico2.Retorno.TRetConsStatServ)XmlUtil.Deserialize<XmlSchemas.NfeStatusServico2.Retorno.TRetConsStatServ>(result.OuterXml);

                return retorno.cStat == "107";
            }
            catch (Exception e)
            {
                log.Error(e);
                return false;
            }
        }
    }
}