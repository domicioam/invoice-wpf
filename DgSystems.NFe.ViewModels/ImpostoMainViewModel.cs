using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using DgSystems.NFe.Core.Cadastro;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using NFe.Core.Cadastro.Imposto;
using NFe.Core.Interfaces;
using NFe.Core.Messaging;
using NFe.WPF.Events;

namespace DgSystems.NFe.ViewModels
{
    public class ImpostoMainViewModel
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IDialogService _dialogService;
        private readonly IGrupoImpostosRepository _grupoImpostosRepository;
        public ObservableCollection<GrupoImpostos> Impostos { get; set; }

        public ICommand LoadedCmd { get; set; }
        public ICommand AlterarImpostoCmd { get; set; }
        public ICommand RemoverImpostoCmd { get; set; }


        public ImpostoMainViewModel(IGrupoImpostosRepository grupoImpostosRepository, IDialogService dialogService)
        {
            LoadedCmd = new RelayCommand(LoadedCmd_Execute, null);
            Impostos = new ObservableCollection<GrupoImpostos>();
            AlterarImpostoCmd = new RelayCommand<GrupoImpostos>(AlterarImpostoCmd_Execute, null);
            RemoverImpostoCmd = new RelayCommand<GrupoImpostos>(RemoverImpostoCmd_Execute, null);

            MessagingCenter.Subscribe<ImpostoViewModel, ImpostoAdicionadoEvent>(this, nameof(ImpostoAdicionadoEvent), (s, e) =>
            {
                ImpostoVM_ImpostoAdicionadoEvent();
            });

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
                await _dialogService.ShowError("Ocorreu um erro ao remover o imposto desejado.", "Erro", "OK", null);
            }
        }

        private void AlterarImpostoCmd_Execute(GrupoImpostos obj)
        {
            var impostoViewModel = new ImpostoViewModel(new GrupoImpostosRepository());
            impostoViewModel.AlterarImposto(obj);
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
