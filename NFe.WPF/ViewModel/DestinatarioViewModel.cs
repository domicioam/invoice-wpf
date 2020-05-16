using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using EmissorNFe.Model;
using EmissorNFe.View.Destinatario;
using GalaSoft.MvvmLight.Command;
using MediatR;
using NFe.Core.Cadastro;
using NFe.Core.Cadastro.Destinatario;
using NFe.Core.Cadastro.Emissor;
using NFe.Core.Entitities;
using NFe.Core.NotasFiscais;
using NFe.WPF.Commands;
using NFe.WPF.Events;
using NFe.WPF.ViewModel.Base;

namespace NFe.WPF.ViewModel
{
    public class DestinatarioViewModel : ViewModelBaseValidation, IRequestHandler<AlterarDestinatarioCommand>
    {
        private DestinatarioModel _destinatarioParaSalvar;
        private IEstadoService _estadoService;
        private IEmissorService _emissorService;
        private IDestinatarioService _destinatarioService;
        private IMunicipioService _municipioService;
        private IMediator _mediator;

        public ICommand SalvarDestinatarioCmd { get; set; }
        public ICommand UfSelecionadoCmd { get; set; }
        public ICommand LoadedCmd { get; set; }
        public ICommand ClosedCmd { get; set; }

        public DestinatarioModel DestinatarioParaSalvar
        {
            get
            {
                return _destinatarioParaSalvar;
            }
            set
            {
                SetProperty(ref _destinatarioParaSalvar, value);
            }
        }

        public ObservableCollection<EstadoEntity> Estados { get; set; }
        public ObservableCollection<MunicipioEntity> Municipios { get; set; }

        public DestinatarioViewModel(IEstadoService estadoService, IEmissorService emissorService, IDestinatarioService destinatarioService, IMunicipioService municipioService, IMediator mediator)
        {
            SalvarDestinatarioCmd = new RelayCommand<Window>(SalvarDestinatarioCmd_Execute, null);
            LoadedCmd = new RelayCommand<bool>(LoadedCmd_Execute, null);
            ClosedCmd = new RelayCommand(ClosedCmd_Execute, null);
            UfSelecionadoCmd = new RelayCommand(UfSelecionadoCmd_Execute, null);
            Estados = new ObservableCollection<EstadoEntity>();
            Municipios = new ObservableCollection<MunicipioEntity>();
            _estadoService = estadoService;
            _emissorService = emissorService;
            _destinatarioService = destinatarioService;
            _municipioService = municipioService;
            _mediator = mediator;
        }

        internal void RemoverDestinatario(DestinatarioModel destinatarioVO)
        {
            _destinatarioService.ExcluirDestinatario(destinatarioVO.Id);
            var theEvent = new DestinatarioSalvoEvent();
            _mediator.Publish(theEvent);
        }

        private void ClosedCmd_Execute()
        {
            DestinatarioParaSalvar = null;
        }

        internal void AlterarDestinatario(DestinatarioModel destinatario)
        {
            DestinatarioParaSalvar = destinatario;

            switch (destinatario.TipoDestinatario)
            {
                case TipoDestinatario.PessoaFisica:
                    DestinatarioParaSalvar.CPF = destinatario.Documento;
                    break;
                case TipoDestinatario.PessoaJuridica:
                    DestinatarioParaSalvar.CNPJ = destinatario.Documento;
                    break;
                case TipoDestinatario.Estrangeiro:
                    DestinatarioParaSalvar.IdEstrangeiro = destinatario.Documento;
                    break;
            }

            var app = Application.Current;
            var mainWindow = app.MainWindow;

            new DestinatarioWindow(destinatario.IsNFe) { Owner = mainWindow }.ShowDialog();
        }

        private void UfSelecionadoCmd_Execute()
        {
            if (DestinatarioParaSalvar == null)
                return;

            var municipios = _municipioService.GetMunicipioByUf(DestinatarioParaSalvar.Endereco.UF);
            Municipios.Clear();

            foreach (var m in municipios)
            {
                Municipios.Add(m);
            }
        }

        private void LoadedCmd_Execute(bool isNFe)
        {
            if (DestinatarioParaSalvar == null)
            {
                DestinatarioParaSalvar = new DestinatarioModel() { IsNFe = isNFe };
            }
            else
            {
                DestinatarioParaSalvar.IsNFe = isNFe;
            }

            var estados = _estadoService.GetEstados();

            foreach (var estado in estados)
            {
                Estados.Add(estado);
            }

            var emitenteUf = _emissorService.GetEmissor().Endereco.UF;
            DestinatarioParaSalvar.Endereco.UF = emitenteUf;

            UfSelecionadoCmd_Execute();
        }

        private void SalvarDestinatarioCmd_Execute(Window window)
        {
            DestinatarioParaSalvar.ValidateModel();

            if (!DestinatarioParaSalvar.HasErrors)
            {
                if (DestinatarioParaSalvar.Id != 0)
                {
                    var destinatarioEnt = _destinatarioService.GetDestinatarioByID(DestinatarioParaSalvar.Id);

                    switch (DestinatarioParaSalvar.TipoDestinatario)
                    {
                        case TipoDestinatario.PessoaFisica:
                            destinatarioEnt.Documento = DestinatarioParaSalvar.CPF;
                            break;
                        case TipoDestinatario.PessoaJuridica:
                            destinatarioEnt.Documento = DestinatarioParaSalvar.CNPJ;
                            break;
                        case TipoDestinatario.Estrangeiro:
                            destinatarioEnt.Documento = DestinatarioParaSalvar.IdEstrangeiro;
                            break;
                    }

                    destinatarioEnt.Email = DestinatarioParaSalvar.Email;
                    destinatarioEnt.TipoDestinatario = (int)DestinatarioParaSalvar.TipoDestinatario;
                    destinatarioEnt.NomeRazao = DestinatarioParaSalvar.NomeRazao;
                    destinatarioEnt.Telefone = DestinatarioParaSalvar.Telefone;
                    destinatarioEnt.InscricaoEstadual = DestinatarioParaSalvar.InscricaoEstadual;

                    if (!string.IsNullOrWhiteSpace(DestinatarioParaSalvar.Endereco.Logradouro))
                    {
                        destinatarioEnt.Endereco = new EnderecoDestinatarioEntity
                        {
                            Id = DestinatarioParaSalvar.Endereco.Id,
                            Bairro = DestinatarioParaSalvar.Endereco.Bairro,
                            Logradouro = DestinatarioParaSalvar.Endereco.Logradouro,
                            Municipio = DestinatarioParaSalvar.Endereco.Municipio,
                            Numero = DestinatarioParaSalvar.Endereco.Numero,
                            UF = DestinatarioParaSalvar.Endereco.UF
                        };
                    }

                    _destinatarioService.Salvar(destinatarioEnt);
                }
                else
                {
                    var destinatarioEntity = (DestinatarioEntity)DestinatarioParaSalvar;
                    _destinatarioService.Salvar(destinatarioEntity);
                }

                var theEvent = new DestinatarioSalvoEvent(DestinatarioParaSalvar);
                _mediator.Publish(theEvent);

                DestinatarioParaSalvar = null;
                window.Close();
            }
        }

        public Task<Unit> Handle(AlterarDestinatarioCommand request, CancellationToken cancellationToken)
        {
            AlterarDestinatario(request.DestinatarioSelecionado);
            return Unit.Task;
        }
    }
}
