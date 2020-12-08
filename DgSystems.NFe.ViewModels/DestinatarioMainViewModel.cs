using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using EmissorNFe.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using NFe.Core.Cadastro.Destinatario;
using NFe.Core.Messaging;
using NFe.WPF.Commands;
using NFe.WPF.Events;

namespace NFe.WPF.ViewModel
{
    public class DestinatarioMainViewModel : ViewModelBase
    {
        private IDestinatarioService _destinatarioService;
        private DestinatarioViewModel _destinatarioViewModel;

        public ObservableCollection<DestinatarioModel> Destinatarios { get; set; }

        public ICommand LoadedCmd { get; set; }
        public ICommand ExcluirDestinatarioCmd { get; set; }
        public ICommand AlterarDestinatarioCmd { get; set; }

        public DestinatarioMainViewModel(IDestinatarioService destinatarioService, DestinatarioViewModel destinatarioViewModel)
        {
            LoadedCmd = new RelayCommand(LoadedCmd_Execute, null);
            AlterarDestinatarioCmd = new RelayCommand<DestinatarioModel>(AlterarDestinatarioCmd_Execute, null);
            ExcluirDestinatarioCmd = new RelayCommand<DestinatarioModel>(ExcluirDestinatarioCmd_Execute, null);
            Destinatarios = new ObservableCollection<DestinatarioModel>();

            _destinatarioService = destinatarioService;
            _destinatarioViewModel = destinatarioViewModel;

            MessagingCenter.Subscribe<DestinatarioViewModel, DestinatarioSalvoEvent>(this, nameof(DestinatarioSalvoEvent), (s, e) =>
            {
                DestinatarioVM_DestinatarioSalvoEvent(e.Destinatario);
            });
        }

        private void AlterarDestinatarioCmd_Execute(DestinatarioModel obj)
        {
            _destinatarioViewModel.DestinatarioParaSalvar = obj;

            var command = new AlterarDestinatarioCommand(_destinatarioViewModel);
            MessagingCenter.Send(this, nameof(AlterarDestinatarioCommand), command);
        }

        private void ExcluirDestinatarioCmd_Execute(DestinatarioModel destinatarioVO)
        {
            _destinatarioService.ExcluirDestinatario(destinatarioVO.Id);

            var theEvent = new DestinatarioSalvoEvent();
            MessagingCenter.Send(this, nameof(DestinatarioSalvoEvent), theEvent);
        }

        private void DestinatarioVM_DestinatarioSalvoEvent(DestinatarioModel DestinatarioParaSalvar)
        {
            PopularListaDestinatarios();
        }

        private void LoadedCmd_Execute()
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() => PopularListaDestinatarios()));
        }

        private async void PopularListaDestinatarios()
        {
            Destinatarios.Clear();

            var destinatariosDB = await _destinatarioService.GetAllAsync();

            foreach(var dest in destinatariosDB)
            {
                Destinatarios.Add((DestinatarioModel)dest);
            }
        }
    }
}
