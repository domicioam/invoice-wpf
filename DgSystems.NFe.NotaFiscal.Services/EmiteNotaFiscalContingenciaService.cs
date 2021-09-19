using NFe.Core.Cadastro.Certificado;
using NFe.Core.Cadastro.Configuracoes;
using NFe.Core.Domain;
using NFe.Core.Entitities;
using NFe.Core.Interfaces;
using NFe.Core.NotasFiscais;
using NFe.Core.NotasFiscais.Sefaz.NfeConsulta2;
using NFe.Core.NotasFiscais.Services;
using NFe.Core.Utils.Conversores;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace NFe.Core.Sefaz.Facades
{
    public class EmiteNotaFiscalContingenciaFacade : IEmiteNotaFiscalContingenciaFacade
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IConfiguracaoRepository _configuracaoService;
        private readonly INotaFiscalRepository _notaFiscalRepository;

        private readonly IEmitenteRepository _emissorService;
        private readonly IConsultarNotaFiscalService _nfeConsulta;
        private readonly CertificadoService _certificadoService;
        private readonly InutilizarNotaFiscalService _notaInutilizadaFacade;
        private readonly ICancelaNotaFiscalService _cancelaNotaFiscalService;

        public EmiteNotaFiscalContingenciaFacade(IConfiguracaoRepository configuracaoService, INotaFiscalRepository notaFiscalRepository, IEmitenteRepository emissorService, IConsultarNotaFiscalService nfeConsulta, CertificadoService certificadoService, InutilizarNotaFiscalService notaInutilizadaFacade, ICancelaNotaFiscalService cancelaNotaFiscalService)
        {
            _configuracaoService = configuracaoService;
            _notaFiscalRepository = notaFiscalRepository;
            _emissorService = emissorService;
            _nfeConsulta = nfeConsulta;
            _certificadoService = certificadoService;
            _notaInutilizadaFacade = notaInutilizadaFacade;
            _cancelaNotaFiscalService = cancelaNotaFiscalService;
        }

        public async Task<NotaFiscal> SaveNotaFiscalContingenciaAsync(X509Certificate2 certificado, ConfiguracaoEntity config, NotaFiscal notaFiscal, string cscId, string csc, string nFeNamespaceName)
        {
            notaFiscal = await SetContingenciaFieldsAsync(config, notaFiscal);
            var xmlNFeContingencia = new XmlNFe(notaFiscal, nFeNamespaceName, certificado, cscId, csc);
            notaFiscal.QrCodeUrl = xmlNFeContingencia.QrCode.ToString();

            _notaFiscalRepository.Salvar(notaFiscal, xmlNFeContingencia.XmlNode.OuterXml);
            return notaFiscal;
        }

        private async Task<NotaFiscal> SetContingenciaFieldsAsync(ConfiguracaoEntity config, NotaFiscal notaFiscal)
        {
            var notaContingencia = await NotaFiscal.CriarNotaFiscalContingenciaAsync(notaFiscal.Emitente, notaFiscal.Destinatario, notaFiscal.Transporte,
                notaFiscal.TotalNFe, notaFiscal.InfoAdicional, notaFiscal.Produtos, notaFiscal.Identificacao.UF, notaFiscal.Identificacao.DataHoraEmissao,
                notaFiscal.Identificacao.Modelo, notaFiscal.Identificacao.TipoEmissao, notaFiscal.Identificacao.Ambiente,
                notaFiscal.Identificacao.NaturezaOperacao, notaFiscal.Identificacao.FinalidadeEmissao, notaFiscal.Identificacao.FormatoImpressao,
                notaFiscal.Identificacao.PresencaComprador, notaFiscal.Identificacao.FinalidadeConsumidor, new NotaFiscalService(_configuracaoService), config.DataHoraEntradaContingencia,
                config.JustificativaContingencia, notaFiscal.Pagamentos);

            return notaContingencia;
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
            var modelo = notaParaCancelar.Modelo.Equals("55") ? Modelo.Modelo55 : Modelo.Modelo65;

            var result = _nfeConsulta.ConsultarNotaFiscal(notaParaCancelar.Chave, codigoUf, certificado, modelo);
            var codigoUfEnum = (CodigoUfIbge)Enum.Parse(typeof(CodigoUfIbge), emitente.Endereco.UF);

            if (result.IsEnviada)
            {
                var dadosNotaParaCancelar = new DadosNotaParaCancelar(ufEmissor, codigoUfEnum, emitente.CNPJ, notaParaCancelar.Chave, 
                    result.Protocolo.Numero, modelo);

                _cancelaNotaFiscalService.CancelarNotaFiscal(dadosNotaParaCancelar, "Nota duplicada em contingência");
            }
            else
            {
                var resultadoInutilizacao = _notaInutilizadaFacade.InutilizarNotaFiscal(codigoUfEnum, emitente.CNPJ,
                    modelo, notaParaCancelar.Serie, notaParaCancelar.Numero,
                    notaParaCancelar.Numero);

                if (resultadoInutilizacao.Status != NotasFiscais.Sefaz.NfeInutilizacao2.Status.ERRO)
                    _notaFiscalRepository.ExcluirNota(notaParaCancelar.Chave);
            }
        }

    }
}