using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using NFe.Core.Cadastro.Imposto;
using NFe.Core.Interfaces;
using NFe.Core.Messaging;
using NFe.WPF.Events;

namespace NFe.WPF.ViewModel
{
    public class ImpostoMainViewModel
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private IDialogService _dialogService;
        private IGrupoImpostosRepository _grupoImpostosRepository;
        private ImpostoViewModel _impostoViewModel;
        public ObservableCollection<GrupoImpostos> Impostos { get; set; }

        public ICommand LoadedCmd { get; set; }
        public ICommand AlterarImpostoCmd { get; set; }
        public ICommand RemoverImpostoCmd { get; set; }


        public ImpostoMainViewModel(IGrupoImpostosRepository grupoImpostosRepository, ImpostoViewModel impostoViewModel, IDialogService dialogService)
        {
            LoadedCmd = new RelayCommand(LoadedCmd_Execute, null);
            Impostos = new ObservableCollection<GrupoImpostos>();
            AlterarImpostoCmd = new RelayCommand<GrupoImpostos>(AlterarImpostoCmd_Execute, null);
            RemoverImpostoCmd = new RelayCommand<GrupoImpostos>(RemoverImpostoCmd_Execute, null);

            MessagingCenter.Subscribe<ImpostoViewModel, ImpostoAdicionadoEvent>(this, nameof(ImpostoAdicionadoEvent), (s, e) =>
            {
                ImpostoVM_ImpostoAdicionadoEvent();
            });

            _impostoViewModel = impostoViewModel;
            _grupoImpostosRepository = grupoImpostosRepository;
            _dialogService = dialogService;
        }

        private async void RemoverImpostoCmd_Execute(GrupoImpostos obj)
        {
            try
            {
                var grupoImpostos = _grupoImpostosRepository.GetById(obj.Id);
                _grupoImpostosRepository.Excluir(grupoImpostos);
                PopularListaImpostos();
            }
            catch(Exception e)
            {
                log.Error(e);
                await _dialogService.ShowError("Ocorreu um erro ao remover o produto desejado.", "Erro", "OK", null);
            }
        }

        private void AlterarImpostoCmd_Execute(GrupoImpostos obj)
        {
            _impostoViewModel.AlterarImposto(obj);
        }

        private void ImpostoVM_ImpostoAdicionadoEvent()
        {
            PopularListaImpostos();
        }

        private void LoadedCmd_Execute()
        {
            PopularListaImpostos();
        }

        private void PopularListaImpostos()
        {
            Impostos.Clear();

            var impostos = _grupoImpostosRepository.GetAll();

            foreach (var imposto in impostos)
            {
                Impostos.Add(imposto);
            }
        }
    }
}
