using EmissorNFe.Model;
using GalaSoft.MvvmLight.Command;
using NFe.Core.Cadastro.Destinatario;
using NFe.Core.Cadastro.Emissor;
using NFe.Core.Domain;
using NFe.Core.Entitities;
using NFe.Core.Interfaces;
using NFe.Core.Messaging;
using NFe.WPF.Events;
using NFe.WPF.ViewModel.Base;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace NFe.WPF.ViewModel
{
    public class DestinatarioViewModel : ViewModelBaseValidation
    {
        public DestinatarioViewModel(IEstadoRepository estadoService, IEmitenteRepository emissorService, IDestinatarioRepository destinatarioService, IMunicipioRepository municipioService)
        {
            SalvarDestinatarioCmd = new RelayCommand<Window>(SalvarDestinatarioCmd_Execute, null);
            LoadedCmd = new RelayCommand<bool>(LoadedCmd_Execute, null);
            ClosedCmd = new RelayCommand(ClosedCmd_Execute, null);
            UfSelecionadoCmd = new RelayCommand(UfSelecionadoCmd_Execute, null);
            Estados = new ObservableCollection<EstadoEntity>();
            Municipios = new ObservableCollection<MunicipioEntity>();
            _estadoRepository = estadoService;
            _emissorService = emissorService;
            _destinatarioService = destinatarioService;
            _municipioService = municipioService;
        }

        private DestinatarioModel _destinatarioParaSalvar;
        private IEstadoRepository _estadoRepository;
        private IEmitenteRepository _emissorService;
        private IDestinatarioRepository _destinatarioService;
        private IMunicipioRepository _municipioService;

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

                if (value != null)
                {
                    switch (value.TipoDestinatario)
                    {
                        case TipoDestinatario.PessoaFisica:
                            DestinatarioParaSalvar.CPF = value.Documento;
                            break;
                        case TipoDestinatario.PessoaJuridica:
                            DestinatarioParaSalvar.CNPJ = value.Documento;
                            break;
                        case TipoDestinatario.Estrangeiro:
                            DestinatarioParaSalvar.IdEstrangeiro = value.Documento;
                            break;
                    }
                }
            }
        }

        public ObservableCollection<EstadoEntity> Estados { get; set; }
        public ObservableCollection<MunicipioEntity> Municipios { get; set; }

        private void ClosedCmd_Execute()
        {
            DestinatarioParaSalvar = null;
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
            var estados = _estadoRepository.GetEstados();

            foreach (var estado in estados)
            {
                Estados.Add(estado);
            }

            if (DestinatarioParaSalvar == null)
            {
                DestinatarioParaSalvar = new DestinatarioModel() { IsNFe = isNFe };
                DestinatarioParaSalvar.Endereco.UF = _emissorService.GetEmissor().Endereco.UF;
                UfSelecionadoCmd_Execute();
            }
            else
            {
                var municipios = _municipioService.GetMunicipioByUf(DestinatarioParaSalvar.Endereco.UF);
                Municipios.Clear();
                foreach (var m in municipios)
                {
                    Municipios.Add(m);
                }

                DestinatarioParaSalvar.IsNFe = isNFe;
            }
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
                            UF = DestinatarioParaSalvar.Endereco.UF,
                            CEP = DestinatarioParaSalvar.Endereco.CEP
                        };
                    }

                    _destinatarioService.Salvar(destinatarioEnt);
                }
                else
                {
                    var destinatarioEntity = (DestinatarioEntity)DestinatarioParaSalvar;
                    _destinatarioService.Salvar(destinatarioEntity);
                }

                var theEvent = new DestinatarioSalvoEvent() { Destinatario = DestinatarioParaSalvar };
                MessagingCenter.Send(this, nameof(DestinatarioSalvoEvent), theEvent);

                DestinatarioParaSalvar = null;
                window.Close();
            }
        }
    }
}
