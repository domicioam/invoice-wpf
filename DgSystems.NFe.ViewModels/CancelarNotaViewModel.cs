using DgSystems.NFe.ViewModels.Commands;
using GalaSoft.MvvmLight.Command;
using NFe.Core.Cadastro.Certificado;
using NFe.Core.Cadastro.Configuracoes;
using NFe.Core.Cadastro.Emissor;
using NFe.Core.Domain;
using NFe.Core.Interfaces;
using NFe.Core.Messaging;
using NFe.Core.NotasFiscais.Sefaz.NfeRecepcaoEvento;
using NFe.Core.NotasFiscais.Services;
using NFe.Core.Sefaz.Facades;
using NFe.Core.Utils;
using NFe.Core.Utils.Assinatura;
using NFe.WPF.Events;
using NFe.WPF.ViewModel.Base;
using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.X509Certificates;
using System.Windows;
using System.Windows.Input;

namespace NFe.WPF.ViewModel
{
    public class CancelarNotaViewModel : ViewModelBaseValidation
    {
        public ICommand EnviarCancelamentoCmd { get; set; }

        private string _motivoCancelamento;
        private readonly IConfiguracaoRepository _configuracaoService;
        private readonly IEmitenteRepository _emissorService;
        private readonly InutilizarNotaFiscalService _notaInutilizadaFacade;
        private readonly INotaFiscalRepository _notaFiscalRepository;
        private readonly ICertificadoService _certificateManager;
        private readonly ICancelaNotaFiscalService _cancelaNotaFiscalService;

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

        private void EnviarCancelamentoCmd_Execute(Window window)
        {
            ValidateModel();

            if (!HasErrors)
            {
                var dadosNotaParaCancelar = new DadosNotaParaCancelar()
                {
                    chaveNFe = Chave,
                    cnpjEmitente = Cnpj,
                    codigoUf = CodigoUF,
                    ufEmitente = UF,
                    modeloNota = ModeloNota,
                    protocoloAutorizacao = Protocolo
                };

                var mensagemRetorno = _cancelaNotaFiscalService.CancelarNotaFiscal(dadosNotaParaCancelar, MotivoCancelamento);

                if (mensagemRetorno.Status == StatusEvento.SUCESSO)
                {
                    var notaCancelada = _notaFiscalRepository.GetNotaFiscalByChave(Chave);

                    var theEvent = new NotaFiscalCanceladaEvent() { NotaFiscal = notaCancelada };
                    MessagingCenter.Send(this, nameof(NotaFiscalCanceladaEvent), theEvent);

                    MessageBox.Show("Nota cancelada com sucesso!", "Sucesso!", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(mensagemRetorno.Mensagem, "Erro!", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                window.Close();
            }
        }

        internal void CancelarNotaFiscal(Modelo modelo, Chave chave, string protocolo, StatusEnvio status, string numero, string serie)
        {
            if (status.Equals("Enviada"))
            {
                EnviarCancelamentoNotaFiscal(modelo, chave.ToString(), protocolo);
            }
            else
            {
                InutilizarNotaFiscal(modelo, numero, serie, chave);
            }
        }

        private void InutilizarNotaFiscal(Modelo modelo, string numero, string serie, Chave chave)
        {
            var config = _configuracaoService.GetConfiguracao();

            var emitente = _emissorService.GetEmissor();
            var codigoUF = (CodigoUfIbge)Enum.Parse(typeof(CodigoUfIbge), emitente.Endereco.UF);
            var modeloNota = modelo;

            if (modeloNota == Modelo.Modelo55) //NF-e
            {
                string proximoNumNFe = config.ProximoNumNFe;
                var numAtual = (int.Parse(proximoNumNFe) - 1).ToString();

                //Se o número atual não tiver mudado, significa que ela não foi enviada e pode ser excluída sem inutilizar.
                if (numero == numAtual)
                {
                    config.ProximoNumNFe = numAtual;

                    _notaFiscalRepository.ExcluirNota(chave.ToString());

                    var theEvent = new NotaFiscalInutilizadaEvent() { Chave = chave };
                    MessagingCenter.Send(this, nameof(NotaFiscalInutilizadaEvent), theEvent);

                    _configuracaoService.Salvar(config);
                }
                else //caso o número atual seja diferente, é necessário inutilizar
                {
                    var mensagemRetorno = _notaInutilizadaFacade.InutilizarNotaFiscal(emitente.Endereco.UF, codigoUF, emitente.CNPJ, modeloNota,
                        serie, numero, numero);

                    if (mensagemRetorno.Status != Core.NotasFiscais.Sefaz.NfeInutilizacao2.Status.ERRO)
                    {
                        _notaFiscalRepository.ExcluirNota(chave.ToString());

                        var theEvent = new NotaFiscalInutilizadaEvent() { Chave = chave };
                        MessagingCenter.Send(this, nameof(NotaFiscalInutilizadaEvent), theEvent);

                        _configuracaoService.Salvar(config);
                    }
                    else
                    {
                        MessageBox.Show("Houve um erro ao tentar cancelar a nota. Tente novamente e, se o erro persistir, contate o suporte.\n\n" + mensagemRetorno.Mensagem, "Erro!", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            else //NFC-e
            {
                string proximoNumNFCe = config.ProximoNumNFe;
                var numAtual = (int.Parse(proximoNumNFCe) - 1).ToString();

                if (numero == numAtual)
                {
                    config.ProximoNumNFCe = numAtual;

                    _notaFiscalRepository.ExcluirNota(chave.ToString());

                    var theEvent = new NotaFiscalInutilizadaEvent() { Chave = chave };
                    MessagingCenter.Send(this, nameof(NotaFiscalInutilizadaEvent), theEvent);

                    _configuracaoService.Salvar(config);
                }
                else
                {
                    var mensagemRetorno = _notaInutilizadaFacade.InutilizarNotaFiscal(emitente.Endereco.UF, codigoUF, emitente.CNPJ, modeloNota,
                         serie, numero, numero);

                    if (mensagemRetorno.Status != Core.NotasFiscais.Sefaz.NfeInutilizacao2.Status.ERRO)
                    {
                        _notaFiscalRepository.ExcluirNota(chave.ToString());

                        var theEvent = new NotaFiscalInutilizadaEvent() { Chave = chave };
                        MessagingCenter.Send(this, nameof(NotaFiscalInutilizadaEvent), theEvent);

                        _configuracaoService.Salvar(config);
                    }
                    else
                    {
                        MessageBox.Show("Houve um erro ao tentar cancelar a nota. Tente novamente e, se o erro persistir, contate o suporte.\n\n" + mensagemRetorno.Mensagem, "Erro!", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
        }

        private void EnviarCancelamentoNotaFiscal(Modelo modelo, string chave, string protocolo)
        {
            X509Certificate2 certificado = _certificateManager.GetX509Certificate2();

            var modeloNota = modelo;
            var config = _configuracaoService.GetConfiguracao();

            var emitente = _emissorService.GetEmissor();
            var codigoUF = (CodigoUfIbge)Enum.Parse(typeof(CodigoUfIbge), emitente.Endereco.UF);

            UF = emitente.Endereco.UF;
            CodigoUF = codigoUF;
            Cnpj = emitente.CNPJ;
            Chave = chave;
            Protocolo = protocolo;
            ModeloNota = modeloNota;
            Certificado = certificado;

            var command = new CancelarNotaFiscalCommand(this);
            MessagingCenter.Send(this, nameof(CancelarNotaFiscalCommand), command);
        }

        public CancelarNotaViewModel(IConfiguracaoRepository configuracaoService, IEmitenteRepository emissorService, InutilizarNotaFiscalService notaInutilizadaFacade, INotaFiscalRepository notaFiscalRepository, ICertificadoService certificateManager, ICancelaNotaFiscalService cancelaNotaFiscalService)
        {
            EnviarCancelamentoCmd = new RelayCommand<Window>(EnviarCancelamentoCmd_Execute, null);
            _configuracaoService = configuracaoService;
            _emissorService = emissorService;
            _notaInutilizadaFacade = notaInutilizadaFacade;
            _notaFiscalRepository = notaFiscalRepository;
            _certificateManager = certificateManager;
            _cancelaNotaFiscalService = cancelaNotaFiscalService;
        }
    }
}
