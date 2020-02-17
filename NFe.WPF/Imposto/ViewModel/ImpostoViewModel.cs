using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using EmissorNFe.Imposto;
using GalaSoft.MvvmLight.Command;
using NFe.Core.Cadastro.Imposto;
using NFe.Core.Entitities;
using NFe.Core.Utils;
using NFe.WPF.ViewModel.Base;

namespace NFe.WPF.ViewModel
{
    public delegate void ImpostoAdicionadoEventHandler();

    public class ImpostoViewModel : ViewModelBaseValidation
    {
        private ObservableCollection<string> _cstList;

        public event ImpostoAdicionadoEventHandler ImpostoAdicionadoEvent = delegate { };

        public int Id { get; set; }
        public string CFOP { get; set; }
        public string Descricao { get; set; }

        public ICMS ICMS { get; set; }
        public ICMSST ICMSST { get; set; }
        public IPI IPI { get; set; }
        public PIS PIS { get; set; }
        public COFINS COFINS { get; set; }

        private bool _isShowRegime;
        private bool _isShowReducao;

        private Imposto _imposto;

        public Imposto Imposto
        {
            get { return _imposto; }
            set { SetProperty(ref _imposto, value); }
        }

        internal void AlterarImposto(GrupoImpostos obj)
        {
            Impostos.Clear();

            CFOP = obj.CFOP;
            Descricao = obj.Descricao;
            Id = obj.Id;

            foreach(var imposto in obj.Impostos)
            {
                Impostos.Add(new Imposto() { Aliquota = imposto.Aliquota, CST = imposto.CST, Nome = imposto.TipoImposto.ToString() });
            }

            var app = Application.Current;
            var mainWindow = app.MainWindow;

            new CadastroImpostoWindow() { Owner = mainWindow }.ShowDialog();
        }

        private ObservableCollection<Imposto> _impostos;
        private ImpostoService _impostoService;

        public ObservableCollection<Imposto> Impostos
        {
            get { return _impostos; }
            set { SetProperty(ref _impostos, value); }
        }

        public bool IsShowRegime
        {
            get { return _isShowRegime; }
            set { SetProperty(ref _isShowRegime, value); }
        }

        public bool IsShowReducao
        {
            get { return _isShowReducao; }
            set { SetProperty(ref _isShowReducao, value); }
        }

        public ObservableCollection<string> CstList
        {
            get { return _cstList; }
            set { SetProperty(ref _cstList, value); }
        }

        public ICommand SalvarCmd { get; set; }
        public ICommand CancelarCmd { set; get; }
        public ICommand ImpostoSelecionadoCmd { set; get; }
        public ICommand AdiciionarImpostoCmd { set; get; }

        public ImpostoViewModel(ImpostoService impostoService)
        {
            SalvarCmd = new RelayCommand<Window>(SalvarCmd_Execute, null);
            CancelarCmd = new RelayCommand<object>(CancelarCmd_Execute, null);
            ImpostoSelecionadoCmd = new RelayCommand(ImpostoSelecionadoCmd_Execute, null);
            AdiciionarImpostoCmd = new RelayCommand(AdiciionarImpostoCmd_Execute, null);

            ICMS = new ICMS();
            ICMSST = new ICMSST();
            IPI = new IPI();
            PIS = new PIS();
            COFINS = new COFINS();

            Imposto = new Imposto();
            Impostos = new ObservableCollection<Imposto>();
            _impostoService = impostoService;
        }

        private void AdiciionarImpostoCmd_Execute()
        {
            Impostos.Add(Imposto);
            Imposto = new Imposto();
        }

        private void ImpostoSelecionadoCmd_Execute()
        {
            if (string.IsNullOrEmpty(Imposto.Nome))
            {
                return;
            }

            CstList = new ObservableCollection<string>(CstListManager.GetCstListPorImposto(Imposto.Nome));

            switch (Imposto.Nome)
            {
                case "PIS":
                case "COFINS":
                    IsShowRegime = true;
                    IsShowReducao = false;
                    break;

                case "ICMS":
                    IsShowReducao = true;
                    IsShowRegime = false;
                    break;

                default:
                    IsShowRegime = false;
                    IsShowReducao = false;
                    break;
            }
        }

        private void CancelarCmd_Execute(object obj)
        {
            throw new NotImplementedException();
        }

        private void SalvarCmd_Execute(Window window)
        {
            var grupoImpostos = new GrupoImpostos();

            foreach (var i in Impostos)
            {
                switch (i.Nome)
                {
                    case "ICMS":
                        grupoImpostos.Impostos.Add(new NFe.Core.Entitities.Imposto() { CST = i.CST, Aliquota = i.Aliquota, TipoImposto = TipoImposto.Icms  });
                        break;

                    case "PIS":
                        grupoImpostos.Impostos.Add(new NFe.Core.Entitities.Imposto() { CST = i.CST, Aliquota = i.Aliquota, TipoImposto = TipoImposto.PIS });
                        break;

                    case "COFINS":
                        grupoImpostos.Impostos.Add(new NFe.Core.Entitities.Imposto() { CST = i.CST, Aliquota = i.Aliquota, TipoImposto = TipoImposto.Confins });
                        break;

                    case "IPI":
                        grupoImpostos.Impostos.Add(new NFe.Core.Entitities.Imposto() { CST = i.CST, Aliquota = i.Aliquota, TipoImposto = TipoImposto.IPI });
                        break;
                }
            }

            grupoImpostos.CFOP = CFOP;
            grupoImpostos.Descricao = Descricao;
            grupoImpostos.Id = Id;

            _impostoService.Salvar(grupoImpostos);

            ImpostoAdicionadoEvent();
            window.Close();
        }
    }

    public class ICMS
    {
        public string CST { get; set; }
        public double Aliquota { get; set; }
        public List<string> CstList { get { return new List<string>() { "41", "60" }; } }
    }

    public class ICMSST
    {
        public double Margem { get; set; }
        public double Aliquota { get; set; }
        public double Reducao { get; set; }
    }

    public class IPI
    {
        public string CST { get; set; }
        public double Aliquota { get; set; }
        public List<string> CstList { get { return new List<string>() { "40", "50", "60" }; } }
    }

    public class PIS
    {
        public string CST { get; set; }
        public double Aliquota { get; set; }
        public List<string> CstList { get { return new List<string>() { "04", "05", "06", "07", "08", "09" }; } }
    }

    public class COFINS
    {
        public string CST { get; set; }
        public double Aliquota { get; set; }
        public List<string> CstList { get { return new List<string>() { "01", "40", "50", "60" }; } }
    }

    public class Imposto
    {
        public string Nome { get; set; }
        public string CST { get; set; }
        public string Regime { get; set; }
        public double Aliquota { get; set; }
        public double Reducao { get; set; }
    }
}
