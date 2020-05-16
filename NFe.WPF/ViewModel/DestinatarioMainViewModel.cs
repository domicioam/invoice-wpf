using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using EmissorNFe.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MediatR;
using NFe.Core.Cadastro.Destinatario;
using NFe.WPF.Events;

namespace NFe.WPF.ViewModel
{
    public class DestinatarioMainViewModel : ViewModelBase, INotificationHandler<DestinatarioSalvoEvent>
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
        }

        private void AlterarDestinatarioCmd_Execute(DestinatarioModel obj)
        {
            _destinatarioViewModel.AlterarDestinatario(obj);
        }

        private void ExcluirDestinatarioCmd_Execute(DestinatarioModel destinatarioVO)
        {
            _destinatarioViewModel.RemoverDestinatario(destinatarioVO);
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

        Task INotificationHandler<DestinatarioSalvoEvent>.Handle(DestinatarioSalvoEvent notification, CancellationToken cancellationToken)
        {
            PopularListaDestinatarios();
            return Unit.Task;
        }
    }
}
