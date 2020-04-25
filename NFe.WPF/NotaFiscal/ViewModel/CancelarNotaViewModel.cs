using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.X509Certificates;
using System.Windows;
using System.Windows.Input;
using EmissorNFe.Model;
using EmissorNFe.View.NotaFiscal;
using GalaSoft.MvvmLight.Command;
using NFe.Core.Cadastro.Certificado;
using NFe.Core.Cadastro.Configuracoes;
using NFe.Core.Cadastro.Emissor;
using NFe.Core.Entitities;
using NFe.Core.NotasFiscais;
using NFe.Core.NotasFiscais.Sefaz.NfeRecepcaoEvento;
using NFe.Core.NotasFiscais.Services;
using NFe.Core.Utils.Assinatura;
using NFe.WPF.ViewModel.Base;
using Utils;
using NFe.Core.Interfaces;
using NFe.Core.Sefaz.Facades;

namespace NFe.WPF.ViewModel
{
    public delegate void NotaCanceladaEventHandler(NotaFiscalEntity notaCancelada);
    public delegate void NotaInutilizadaEventHandler(NFCeModel notaInutilizada);

    public class CancelarNotaViewModel : ViewModelBaseValidation
    {
        public event NotaCanceladaEventHandler NotaCanceladaEvent = delegate { };
        public event NotaInutilizadaEventHandler NotaInutilizadaEvent = delegate { };

        public ICommand EnviarCancelamentoCmd { get; set; }

        private string _motivoCancelamento;
        private readonly IConfiguracaoService _configuracaoService;
        private readonly IEmissorService _emissorService;
        private readonly ICertificadoService _certificadoService;
        private readonly InutilizarNotaFiscalFacade _notaInutilizadaFacade;
        private readonly INotaFiscalRepository _notaFiscalRepository;
        private readonly ICertificateManager _certificateManager;
        private readonly ICancelaNotaFiscalFacade _cancelaNotaFiscalService;

        [Required]
        [MinLength(15)]
        [MaxLength(255)]
        public string MotivoCancelamento
        {
            get { return _motivoCancelamento; }
            set { SetProperty(ref _motivoCancelamento, value); }
        }

        public string UF { get; private set; }
        public CodigoUfIbge CodigoUF { get; private set; }
        public string Cnpj { get; private set; }
        public string Chave { get; private set; }
        public string Protocolo { get; private set; }
        public Modelo ModeloNota { get; private set; }
        public X509Certificate2 Certificado { get; private set; }
        public Ambiente Ambiente { get; private set; }

        private void EnviarCancelamentoCmd_Execute(Window window)
        {
            ValidateModel();

            if (!HasErrors)
            {
                var mensagemRetorno = _cancelaNotaFiscalService.CancelarNotaFiscal(UF, CodigoUF, Ambiente, Cnpj, Chave, Protocolo,
                ModeloNota, MotivoCancelamento);

                if (mensagemRetorno.Status == StatusEvento.SUCESSO)
                {
                    var notaCancelada = _notaFiscalRepository.GetNotaFiscalByChave(Chave);
                    NotaCanceladaEvent(notaCancelada);
                    MessageBox.Show("Nota cancelada com sucesso!", "Sucesso!", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(mensagemRetorno.Mensagem, "Erro!", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                window.Close();
            }
        }

        internal void CancelarNotaFiscal(NFCeModel notaFiscal)
        {
            if (notaFiscal.Status.Equals("Enviada"))
            {
                EnviarCancelamentoNotaFiscal(notaFiscal);
            }
            else
            {
                InutilizarNotaFiscal(notaFiscal);
            }
        }

        private void InutilizarNotaFiscal(NFCeModel notaFiscal)
        {
            var config = _configuracaoService.GetConfiguracao();

            var emitente = _emissorService.GetEmissor();
            var codigoUF = (CodigoUfIbge)Enum.Parse(typeof(CodigoUfIbge), emitente.Endereco.UF);
            var ambiente = config.IsProducao ? Ambiente.Producao : Ambiente.Homologacao;
            var modeloNota = notaFiscal.Modelo.Contains("NFC-e") ? Modelo.Modelo65 : Modelo.Modelo55;

            if (modeloNota == Modelo.Modelo55) //NF-e
            {
                string proximoNumNFe = ambiente == Ambiente.Homologacao ? config.ProximoNumNFeHom : config.ProximoNumNFe;
                var numAtual = (int.Parse(proximoNumNFe) - 1).ToString();

                //Se o número atual não tiver mudado, significa que ela não foi enviada e pode ser excluída sem inutilizar.
                if (notaFiscal.Numero == numAtual)
                {
                    if (ambiente == Ambiente.Homologacao)
                    {
                        config.ProximoNumNFeHom = numAtual;
                    }
                    else
                    {
                        config.ProximoNumNFe = numAtual;
                    }

                    var modelo = notaFiscal.Modelo == "NFC-e" ? "65" : "55";
                    _notaFiscalRepository.ExcluirNota(notaFiscal.Chave, ambiente);
                    NotaInutilizadaEvent(notaFiscal);
                    _configuracaoService.Salvar(config);
                }
                else //caso o número atual seja diferente, é necessário inutilizar
                {
                    var mensagemRetorno = _notaInutilizadaFacade.InutilizarNotaFiscal(emitente.Endereco.UF, codigoUF, ambiente, emitente.CNPJ, modeloNota,
                        notaFiscal.Serie, notaFiscal.Numero, notaFiscal.Numero);

                    if (mensagemRetorno.Status != Core.NotasFiscais.Sefaz.NfeInutilizacao2.Status.ERRO)
                    {
                        _notaFiscalRepository.ExcluirNota(notaFiscal.Chave, ambiente);
                        NotaInutilizadaEvent(notaFiscal);
                        _configuracaoService.Salvar(config);
                    }
                    else
                    {
                        MessageBox.Show("Houve um erro ao tentar cancelar a nota. Tente novamente e, se o erro persistir, contate o suporte." + "\n\n" + mensagemRetorno.Mensagem, "Erro!", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            else //NFC-e
            {
                string proximoNumNFCe = ambiente == Ambiente.Homologacao ? config.ProximoNumNFeHom : config.ProximoNumNFe;
                var numAtual = (int.Parse(proximoNumNFCe) - 1).ToString();

                if (notaFiscal.Numero == numAtual)
                {
                    if (ambiente == Ambiente.Homologacao)
                    {
                        config.ProximoNumNFCeHom = numAtual;
                    }
                    else
                    {
                        config.ProximoNumNFCe = numAtual;
                    }

                    var modelo = notaFiscal.Modelo == "NFC-e" ? "65" : "55";
                    _notaFiscalRepository.ExcluirNota(notaFiscal.Chave, ambiente);
                    NotaInutilizadaEvent(notaFiscal);
                    _configuracaoService.Salvar(config);
                }
                else
                {
                    var mensagemRetorno = _notaInutilizadaFacade.InutilizarNotaFiscal(emitente.Endereco.UF, codigoUF, ambiente, emitente.CNPJ, modeloNota,
                         notaFiscal.Serie, notaFiscal.Numero, notaFiscal.Numero);

                    if (mensagemRetorno.Status != Core.NotasFiscais.Sefaz.NfeInutilizacao2.Status.ERRO)
                    {
                        _notaFiscalRepository.ExcluirNota(notaFiscal.Chave, ambiente);
                        NotaInutilizadaEvent(notaFiscal);
                        _configuracaoService.Salvar(config);
                    }
                    else
                    {
                        MessageBox.Show("Houve um erro ao tentar cancelar a nota. Tente novamente e, se o erro persistir, contate o suporte." + "\n\n" + mensagemRetorno.Mensagem, "Erro!", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
        }

        private void EnviarCancelamentoNotaFiscal(NFCeModel notaFiscalModel)
        {
            X509Certificate2 certificado;

            var modeloNota = notaFiscalModel.Modelo.Contains("NFC-e") ? Modelo.Modelo65 : Modelo.Modelo55;
            var config = _configuracaoService.GetConfiguracao();

            var emitente = _emissorService.GetEmissor();
            var codigoUF = (CodigoUfIbge)Enum.Parse(typeof(CodigoUfIbge), emitente.Endereco.UF);
            var ambiente = config.IsProducao ? Ambiente.Producao : Ambiente.Homologacao;

            var certificadoEntity = _certificadoService.GetCertificado();

            if (!string.IsNullOrWhiteSpace(certificadoEntity.Caminho))
            {
                certificado = _certificateManager.GetCertificateByPath(certificadoEntity.Caminho,
                    RijndaelManagedEncryption.DecryptRijndael(certificadoEntity.Senha));
            }
            else
            {
                certificado = _certificateManager.GetCertificateBySerialNumber(certificadoEntity.NumeroSerial, false);
            }

            UF = emitente.Endereco.UF;
            CodigoUF = codigoUF;
            Cnpj = emitente.CNPJ;
            Chave = notaFiscalModel.Chave;
            Protocolo = notaFiscalModel.Protocolo;
            ModeloNota = modeloNota;
            Certificado = certificado;
            Ambiente = ambiente;

            var app = Application.Current;
            var mainWindow = app.MainWindow;

            new CancelarNotaWindow() { Owner = mainWindow }.ShowDialog();
        }

        public CancelarNotaViewModel(IConfiguracaoService configuracaoService, IEmissorService emissorService, ICertificadoService certificadoService, InutilizarNotaFiscalFacade notaInutilizadaFacade, INotaFiscalRepository notaFiscalRepository, ICertificateManager certificateManager, ICancelaNotaFiscalFacade cancelaNotaFiscalService)
        {
            EnviarCancelamentoCmd = new RelayCommand<Window>(EnviarCancelamentoCmd_Execute, null);
            _configuracaoService = configuracaoService;
            _emissorService = emissorService;
            _certificadoService = certificadoService;
            _notaInutilizadaFacade = notaInutilizadaFacade;
            _notaFiscalRepository = notaFiscalRepository;
            _certificateManager = certificateManager;
            _cancelaNotaFiscalService = cancelaNotaFiscalService;
        }
    }
}
