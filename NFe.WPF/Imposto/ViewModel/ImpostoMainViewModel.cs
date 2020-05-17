using System.Collections.ObjectModel;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using NFe.Core.Cadastro.Imposto;
using NFe.Core.Interfaces;

namespace NFe.WPF.ViewModel
{
    public class ImpostoMainViewModel
    {
        private IGrupoImpostosRepository _grupoImpostosRepository;
        private ImpostoViewModel _impostoViewModel;
        public ObservableCollection<GrupoImpostos> Impostos { get; set; }

        public ICommand LoadedCmd { get; set; }
        public ICommand AlterarImpostoCmd { get; set; }


        public ImpostoMainViewModel(IGrupoImpostosRepository grupoImpostosRepository, ImpostoViewModel impostoViewModel)
        {
            LoadedCmd = new RelayCommand(LoadedCmd_Execute, null);
            Impostos = new ObservableCollection<GrupoImpostos>();
            AlterarImpostoCmd = new RelayCommand<GrupoImpostos>(AlterarImpostoCmd_Execute, null);

            impostoViewModel.ImpostoAdicionadoEvent += ImpostoVM_ImpostoAdicionadoEvent;
            _impostoViewModel = impostoViewModel;
            _grupoImpostosRepository = grupoImpostosRepository;
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

            foreach(var imposto in impostos)
            {
                Impostos.Add(imposto);
            }
        }
    }
}
