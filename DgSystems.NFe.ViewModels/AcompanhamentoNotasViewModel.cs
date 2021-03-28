using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using NFe.Core.NotasFiscais.Services;
using NFe.WPF.ViewModel.Base;
using System.ComponentModel;
using System.Collections.Generic;
using System.Windows.Data;
using DgSystems.NFe.ViewModels;
using NFe.Core.Interfaces;
using NFe.WPF.NotaFiscal.ViewModel;
using NFe.WPF.Events;
using NFe.Core.Messaging;

namespace NFe.WPF.ViewModel
{
    public class AcompanhamentoNotasViewModel : ViewModelBaseValidation
    {
        public ICollectionView Acompanhamentos { get; private set; }

        private DateTime _periodoInicial;

        public DateTime PeriodoInicial
        {
            get { return _periodoInicial; }
            set { SetProperty(ref _periodoInicial, value); }
        }

        private DateTime _periodoFinal;
        private INotaFiscalRepository _notaFiscalRepository;

        public DateTime PeriodoFinal
        {
            get { return _periodoFinal; }
            set { SetProperty(ref _periodoFinal, value); }
        }

        public ICommand FiltrarCmd { get; set; }
        public ICommand LoadedCmd { get; set; }

        public AcompanhamentoNotasViewModel(IEnviaNotaFiscalService enviaNotaFiscalService, INotaFiscalRepository notaFiscalRepository)
        {
            LoadedCmd = new RelayCommand(LoadedCmd_Execute, null);
            FiltrarCmd = new RelayCommand(FiltrarCmd_Execute, null);

            MessagingCenter.Subscribe<EnviarNotaAppService, NotaFiscalEnviadaEvent>(this, nameof(NotaFiscalEnviadaEvent), (s, e) =>
            {
                NotaFiscalVM_NotaEnviadaEvent();
            });

            MessagingCenter.Subscribe<CancelarNotaViewModel, NotaFiscalCanceladaEvent>(this, nameof(NotaFiscalCanceladaEvent), (s, e) =>
            {
                CancelarNotaFiscalVM_NotaCanceladaEvent();
            });

            MessagingCenter.Subscribe<OpcoesViewModel, ConfiguracaoAlteradaEvent>(this, nameof(ConfiguracaoAlteradaEvent), (s, e) =>
            {
                OpcoesVM_ConfiguracoesAlteradasEvent();
            });

            MessagingCenter.Subscribe<NotaFiscalMainViewModel, NotaFiscalPendenteReenviadaEvent>(this, nameof(NotaFiscalPendenteReenviadaEvent), (s, e) =>
            {
                NotaFiscalVM_NotaEnviadaEvent();
            });

            _notaFiscalRepository = notaFiscalRepository;
        }

        private void OpcoesVM_ConfiguracoesAlteradasEvent()
        {
            if (PeriodoFinal >= PeriodoInicial)
            {
                AtualizarListaAcompanhamentos();
            }
        }

        private void CancelarNotaFiscalVM_NotaCanceladaEvent()
        {
            if (PeriodoFinal >= PeriodoInicial)
            {
                AtualizarListaAcompanhamentos();
            }
        }

        private void NotaFiscalVM_NotaEnviadaEvent()
        {
            if (PeriodoFinal >= PeriodoInicial)
            {
                AtualizarListaAcompanhamentos();
            }
        }

        private void FiltrarCmd_Execute()
        {
            if (PeriodoFinal >= PeriodoInicial)
            {
                AtualizarListaAcompanhamentos();
            }
        }

        private void LoadedCmd_Execute()
        {
            PeriodoInicial = DateTime.Today;
            PeriodoFinal = DateTime.Today;

            if (PeriodoFinal >= PeriodoInicial)
            {
                AtualizarListaAcompanhamentos();
            }
        }

        public void AtualizarListaAcompanhamentos()
        {
            var notas = _notaFiscalRepository.GetNotasFiscaisPorPeriodo(PeriodoInicial, PeriodoFinal.AddDays(1), false);
            var produtosDb = notas.SelectMany(n => n.Produtos);

            var groups = produtosDb.GroupBy(p => new { p.Codigo, p.ValorUnidadeComercial });
            var acompanhamentos = new List<Acompanhamento>();

            foreach (var group in groups)
            {
                acompanhamentos.Add(new Acompanhamento() { Nome = group.First().Descricao, Quantidade = group.Sum(p => p.QtdeUnidadeComercial), Valor = group.First().ValorUnidadeComercial });
            }

            Acompanhamentos = new ListCollectionView(acompanhamentos);
            Acompanhamentos.GroupDescriptions.Add(new PropertyGroupDescription("Nome"));
            RaisePropertyChanged("Acompanhamentos");
        }
    }
}
