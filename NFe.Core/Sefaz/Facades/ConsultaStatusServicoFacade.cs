using System;
using System.Configuration;
using System.Security.Cryptography.X509Certificates;
using NFe.Core.Cadastro.Certificado;
using NFe.Core.Cadastro.Configuracoes;
using NFe.Core.Cadastro.Emissor;
using NFe.Core.NotasFiscais.Sefaz.NfeStatusServico2;
using NFe.Core.Utils;
using NFe.Core.Utils.Assinatura;

namespace NFe.Core.NotasFiscais.Services
{
    public class ConsultaStatusServicoFacade : IConsultaStatusServicoFacade
    {
        private const string SEFAZ_ENVIRONMENT = "production";
        private readonly ICertificadoService _certificadoService;
        private readonly ICertificateManager _certificateManager;
        private readonly IEmissorService _emissorService;

        public ConsultaStatusServicoFacade(IEmissorService emissorService, ICertificadoService certificadoService,
            ICertificateManager certificateManager)
        {
            _emissorService = emissorService;
            _certificadoService = certificadoService;
            _certificateManager = certificateManager;
        }

        public bool ExecutarConsultaStatus(ConfiguracaoEntity config, Modelo modelo)
        {
            var sefazEnvironment = ConfigurationManager.AppSettings["sefazEnvironment"];

            var ambiente = sefazEnvironment == SEFAZ_ENVIRONMENT ? Ambiente.Producao : Ambiente.Homologacao;
            var codigoUf = (CodigoUfIbge) Enum.Parse(typeof(CodigoUfIbge), _emissorService.GetEmissor().Endereco.UF);

            X509Certificate2 certificado = null;

            var certificadoEntity = _certificadoService.GetCertificado();

            if (certificadoEntity == null)
                return false;

            if (!string.IsNullOrWhiteSpace(certificadoEntity.Caminho))
                certificado = _certificateManager.GetCertificateByPath(certificadoEntity.Caminho,
                    RijndaelManagedEncryption.DecryptRijndael(certificadoEntity.Senha));
            else
                certificado = _certificateManager.GetCertificateBySerialNumber(certificadoEntity.NumeroSerial, false);

            return NFeStatusServico.ExecutarConsultaStatus(codigoUf, ambiente, certificado, modelo);
        }
    }
}