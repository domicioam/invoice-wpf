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
using NFe.Core.Interfaces;
using NFe.WPF.NotaFiscal.ViewModel;
using NFe.WPF.Events;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace NFe.WPF.ViewModel
{
    public class AcompanhamentoNotasViewModel : ViewModelBaseValidation, 
        INotificationHandler<NotaFiscalEnviadaEvent>,
        INotificationHandler<NotaFiscalCanceladaEvent>,
        INotificationHandler<ConfiguracaoAlteradaEvent>,
        IRequestHandler<NotaFiscalPendenteReenviadaEvent>
    {
        public ICollectionView Acompanhamentos { get; private set; }

        private DateTime _periodoInicial;

        public DateTime PeriodoInicial
        {
            get { return _periodoInicial; }
            set { SetProperty(ref _periodoInicial, value); }
        }

        private DateTime _periodoFinal;
        private IEnviaNotaFiscalFacade _enviaNotaFiscalService;
        private INotaFiscalRepository _notaFiscalRepository;

        public DateTime PeriodoFinal
        {
            get { return _periodoFinal; }
            set { SetProperty(ref _periodoFinal, value); }
        }

        public ICommand FiltrarCmd { get; set; }
        public ICommand LoadedCmd { get; set; }

        public AcompanhamentoNotasViewModel(IEnviarNota enviarNotaController, IEnviaNotaFiscalFacade enviaNotaFiscalService, CancelarNotaViewModel cancelarNotaViewModel, OpcoesViewModel opcoesViewModel, NotaFiscalMainViewModel notaFiscalMainViewModel, INotaFiscalRepository notaFiscalRepository)
        {
            LoadedCmd = new RelayCommand(LoadedCmd_Execute, null);
            FiltrarCmd = new RelayCommand(FiltrarCmd_Execute, null);
            _notaFiscalRepository = notaFiscalRepository;


            _enviaNotaFiscalService = enviaNotaFiscalService;
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

        public Task<Unit> Handle(NotaFiscalPendenteReenviadaEvent request, CancellationToken cancellationToken)
        {
            if (PeriodoFinal >= PeriodoInicial)
            {
                AtualizarListaAcompanhamentos();
            }

            return Unit.Task;
        }

        Task INotificationHandler<NotaFiscalEnviadaEvent>.Handle(NotaFiscalEnviadaEvent notification, CancellationToken cancellationToken)
        {
            if (PeriodoFinal >= PeriodoInicial)
            {
                AtualizarListaAcompanhamentos();
            }

            return Unit.Task;
        }

        Task INotificationHandler<NotaFiscalCanceladaEvent>.Handle(NotaFiscalCanceladaEvent notification, CancellationToken cancellationToken)
        {
            if (PeriodoFinal >= PeriodoInicial)
            {
                AtualizarListaAcompanhamentos();
            }

            return Unit.Task;
        }

        Task INotificationHandler<ConfiguracaoAlteradaEvent>.Handle(ConfiguracaoAlteradaEvent notification, CancellationToken cancellationToken)
        {
            if (PeriodoFinal >= PeriodoInicial)
            {
                AtualizarListaAcompanhamentos();
            }

            return Unit.Task;
        }
    }

    public struct Acompanhamento
    {
        public string Nome { get; set; }
        public int Quantidade { get; set; }
        public double Valor { get; set; }
        public string FormaPagamento { get; set; }
    }
}
