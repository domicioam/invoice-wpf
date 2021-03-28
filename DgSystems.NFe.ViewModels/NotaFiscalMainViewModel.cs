using GalaSoft.MvvmLight.CommandWpf;
using NFe.Core.Cadastro.Certificado;
using NFe.Core.Cadastro.Configuracoes;
using NFe.Core.Cadastro.Emissor;
using NFe.Core.Entitities;
using NFe.Core.Events;
using NFe.Core.Interfaces;
using NFe.Core.Messaging;
using NFe.Core.Domain;
using NFe.Core.NotasFiscais.Sefaz.NfeConsulta2;
using NFe.Core.NotasFiscais.Services;
using NFe.Core.Sefaz;
using NFe.Core.Sefaz.Facades;
using NFe.Core.Utils.Conversores;
using NFe.Core.Utils.PDF;
using NFe.Core.Utils.Xml;
using NFe.WPF.Events;
using NFe.WPF.NotaFiscal.Model;
using NFe.WPF.ViewModel;
using NFe.WPF.ViewModel.Base;
using NFe.WPF.ViewModel.Mementos;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;

namespace DgSystems.NFe.ViewModels
{
    public class NotaFiscalMainViewModel : ViewModelBaseValidation
    {
        public NotaFiscalMainViewModel(IEnviaNotaFiscalFacade enviaNotaFiscalService,
            IConfiguracaoService configuracaoService, ICertificadoService certificadoService,
            IProdutoRepository produtoRepository, IConsultaStatusServicoFacade consultaStatusServicoService,
            IEmissorService emissorService,
            VisualizarNotaEnviadaViewModel visualizarNotaEnviadaViewModel,
            EnviarEmailViewModel enviarEmailViewModel,
            INotaFiscalRepository notaFiscalRepository, INFeConsulta nfeConsulta, ICertificadoRepository certificadoRepository)
        {
            LoadedCmd = new RelayCommand(LoadedCmd_Execute, null);
            VisualizarNotaCmd = new RelayCommand<NotaFiscalMemento>(VisualizarNotaCmd_ExecuteAsync, null);
            EnviarNotaNovamenteCmd = new RelayCommand<NotaFiscalMemento>(EnviarNotaNovamenteCmd_ExecuteAsync, null);
            EnviarEmailCmd = new RelayCommand<NotaFiscalMemento>(EnviarEmailCmd_Execute, null);
            MudarPaginaCmd = new RelayCommand<int>(MudarPaginaCmd_Execute, null);

            _enviaNotaFiscalService = enviaNotaFiscalService;
            _notaFiscalRepository = notaFiscalRepository;
            _configuracaoService = configuracaoService;
            _certificadoService = certificadoService;
            _produtoRepository = produtoRepository;
            _consultaStatusServicoService = consultaStatusServicoService;
            _emissorService = emissorService;
            _visualizarNotaEnviadaViewModel = visualizarNotaEnviadaViewModel;
            _enviarEmailViewModel = enviarEmailViewModel;
            _nfeConsulta = nfeConsulta;
            _certificadoRepository = certificadoRepository;

            NotasFiscais = new ObservableCollection<NotaFiscalMemento>();

            SubscribeToEvents();
        }

        static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private string _busyContent;
        private readonly ICertificadoService _certificadoService;
        private readonly IConfiguracaoService _configuracaoService;
        private readonly IConsultaStatusServicoFacade _consultaStatusServicoService;
        private readonly IEmissorService _emissorService;
        private readonly EnviarEmailViewModel _enviarEmailViewModel;

        private bool _isBusy;
        private bool _isLoaded;

        private bool _isNotasPendentesVerificadas;
        private string _mensagensErroContingencia;
        private readonly INFeConsulta _nfeConsulta;
        private readonly ICertificadoRepository _certificadoRepository;
        private readonly INotaFiscalRepository _notaFiscalRepository;

        private readonly IEnviaNotaFiscalFacade _enviaNotaFiscalService;
        private readonly IProdutoRepository _produtoRepository;
        private readonly VisualizarNotaEnviadaViewModel _visualizarNotaEnviadaViewModel;
        private ObservableCollection<NotaFiscalMemento> _notasFiscais;

        public ObservableCollection<NotaFiscalMemento> NotasFiscais
        {
            get { return _notasFiscais; }
            set { SetProperty(ref _notasFiscais, value); }
        }

        public ICommand LoadedCmd { get; set; }
        public ICommand VisualizarNotaCmd { get; set; }
        public ICommand EnviarNotaNovamenteCmd { get; set; }
        public ICommand EnviarNotaPorEmailCmd { get; set; }
        public ICommand EnviarEmailCmd { get; set; }
        public ICommand MudarPaginaCmd { get; set; }

        public bool IsBusy
        {
            get { return _isBusy; }
            set { SetProperty(ref _isBusy, value); }
        }

        public string BusyContent
        {
            get { return _busyContent; }
            set { SetProperty(ref _busyContent, value); }
        }

        private async Task AtualizarNotasPendentesAsync(X509Certificate2 certificado, List<NotaFiscalEntity> notasFiscaisPendentes, string codigoUf)
        {
            if (_isNotasPendentesVerificadas || NotasFiscais.Count == 0)
                return;

            _isNotasPendentesVerificadas = true;

            if (notasFiscaisPendentes.Count == 0) return;

            if (certificado == null)
                throw new ArgumentNullException(nameof(certificado));

            var idsNotasPendentes = notasFiscaisPendentes.Select(n => n.Id);

            foreach (var idNotaPendente in idsNotasPendentes)
            {
                var nota = await ConsultarNotasAsync(idNotaPendente, codigoUf, certificado);

                if (nota == null)
                    continue;

                var notaPendente = NotasFiscais.FirstOrDefault(n => n.Status == "Pendente" && n.Chave == nota.Chave);
                var index = NotasFiscais.IndexOf(notaPendente);

                var notaMemento = new NotaFiscalMemento(nota.Numero,
                    nota.Modelo == "NFC-e" ? Modelo.Modelo65 : Modelo.Modelo55, nota.DataEmissao,
                    nota.DataAutorizacao, nota.Destinatario, nota.UfDestinatario,
                    nota.ValorTotal.ToString("N2", new CultureInfo("pt-BR")), new StatusEnvio((Status)nota.Status).ToString(), nota.Chave);

                NotasFiscais[index] = notaMemento;
            }
        }

        private Task<NotaFiscalEntity> ConsultarNotasAsync(int idNotaFiscalDb, string codigoUf, X509Certificate2 certificado)
        {
            return Task.Run(async () =>
            {
                var notaFiscalDb = _notaFiscalRepository.GetNotaFiscalById(idNotaFiscalDb, true);
                var loadXmlTask = notaFiscalDb.LoadXmlAsync();
                var document = new XmlDocument();
                var modelo = notaFiscalDb.Modelo.Equals("65") ? Modelo.Modelo65 : Modelo.Modelo55;

                var mensagemRetorno = _nfeConsulta.ConsultarNotaFiscal(notaFiscalDb.Chave, codigoUf, certificado, modelo);

                if (!mensagemRetorno.IsEnviada)
                    return null;

                mensagemRetorno.Protocolo.infProt.Id = null;
                var protSerialized = XmlUtil.Serialize(mensagemRetorno.Protocolo, "");

                var doc = XDocument.Parse(protSerialized);
                doc.Descendants().Attributes().Where(a => a.IsNamespaceDeclaration).Remove();

                foreach (var element in doc.Descendants())
                    element.Name = element.Name.LocalName;

                using (var xmlReader = doc.CreateReader())
                {
                    document.Load(xmlReader);
                }

                notaFiscalDb.DataAutorizacao = mensagemRetorno.Protocolo.infProt.dhRecbto;
                notaFiscalDb.Protocolo = mensagemRetorno.Protocolo.infProt.nProt;

                var xml = await loadXmlTask;
                xml = xml.Replace("<protNFe />", document.OuterXml.Replace("TProtNFe", "protNFe"));

                notaFiscalDb.Status = (int)Status.ENVIADA;
                _notaFiscalRepository.Salvar(notaFiscalDb, xml);

                return notaFiscalDb;
            });
        }

        private async void EnviarNotaController_NotaEnviadaEventHandler()
        {
            Task popularListaNfTask = PopularListaNotasFiscaisAsync();
            Task<List<NotaFiscalEntity>> notasFiscaisPendentesTask = _notaFiscalRepository.GetNotasPendentesAsync(false);
            
            var certificado = _certificadoService.GetX509Certificate2();
            var codigoUf = UfToCodigoUfConversor.GetCodigoUf(_emissorService.GetEmissor().Endereco.UF);
            
            await popularListaNfTask;
            var notasFiscaisPendentes = await notasFiscaisPendentesTask;
            await AtualizarNotasPendentesAsync(certificado, notasFiscaisPendentes, codigoUf);
        }

        private async void EnviarNotaNovamenteCmd_ExecuteAsync(NotaFiscalMemento notaPendenteMemento)
        {
            IsBusy = true;
            BusyContent = "Enviando...";

            var config = await _configuracaoService.GetConfiguracaoAsync();
            var modelo = notaPendenteMemento.Tipo == "NFC-e" ? Modelo.Modelo65 : Modelo.Modelo55;

            var app = Application.Current;

            if (!_consultaStatusServicoService.ExecutarConsultaStatus(config, modelo))
            {
                var mensagem = "Serviço continua indisponível. Aguarde o reestabelecimento da conexão e tente novamente.";
                var caption = "Erro de conexão ou serviço indisponível";
                MessageBox.Show(app.MainWindow, mensagem, caption, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var notaFiscalDb = _notaFiscalRepository.GetNotaFiscalByChave(notaPendenteMemento.Chave);
            var notaFiscalBo = _notaFiscalRepository.GetNotaFiscalFromNfeProcXml(await notaFiscalDb.LoadXmlAsync());
            notaFiscalBo.Identificacao.DataHoraEmissao = DateTime.Now;

            foreach (var prod in notaFiscalBo.Produtos)
            {
                var produtoDb = _produtoRepository.GetByCodigo(prod.Codigo);
                prod.Id = produtoDb.Id;
            }

            try
            {
                var certificado = _certificadoRepository.PickCertificateBasedOnInstallationType();
                var xmlNFe = new XmlNFe(notaFiscalBo, "http://www.portalfiscal.inf.br/nfe", certificado, config.CscId, config.Csc);

                _notaFiscalRepository.ExcluirNota(notaPendenteMemento.Chave);
                _enviaNotaFiscalService.EnviarNotaFiscal(notaFiscalBo, config.CscId, config.Csc, certificado, xmlNFe);

                IsBusy = false;

                var mbResult = MessageBox.Show(app.MainWindow, "Nota enviada com sucesso! Deseja imprimí-la?",
                    "Emissão NFe", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);

                if (mbResult == MessageBoxResult.Yes)
                {
                    BusyContent = "Gerando impressão...";
                    IsBusy = true;
                    await GeradorPDF.GerarPdfNotaFiscal(notaFiscalBo);
                }

                IsBusy = false;

                var notaIndex = NotasFiscais.IndexOf(notaPendenteMemento);
                Destinatario destinatario;
                var destinatarioUf = notaFiscalBo.Emitente.Endereco.UF;

                if (notaFiscalBo.Destinatario == null)
                {
                    destinatario = new Destinatario("CONSUMIDOR NÃO IDENTIFICADO");
                }
                else
                {
                    destinatario = notaFiscalBo.Destinatario;
                    destinatarioUf = destinatario.Endereco != null ? destinatario.Endereco.UF : destinatarioUf;
                }

                var valorTotalProdutos = notaFiscalBo.ValorTotalProdutos.ToString("N2", new CultureInfo("pt-BR"));

                NotasFiscais[notaIndex] = new NotaFiscalMemento(notaFiscalBo.Identificacao.Numero,
                    notaFiscalBo.Identificacao.Modelo, notaFiscalBo.Identificacao.DataHoraEmissao,
                    notaFiscalBo.DataHoraAutorização, destinatario.NomeRazao, destinatarioUf, valorTotalProdutos,
                    notaFiscalBo.Identificacao.Status.ToString(), notaFiscalBo.Identificacao.Chave.ToString());

                var theEvent = new NotaFiscalPendenteReenviadaEvent() { NotaFiscal = notaFiscalBo };
                MessagingCenter.Send(this, nameof(NotaFiscalPendenteReenviadaEvent), theEvent);
            }
            catch (Exception e)
            {
                log.Error(e);
                MessageBox.Show(app.MainWindow,
                    "Ocorreram os seguintes erros ao tentar enviar a nota fiscal:\n\n" + e.InnerException.Message,
                    "Erro", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void EnviarEmailCmd_Execute(NotaFiscalMemento notaFiscal)
        {
            _enviarEmailViewModel.EnviarEmail(notaFiscal.Chave);
        }

        private Task<string> GetNotaXmlAsync(string chave)
        {
            var notaDb = _notaFiscalRepository.GetNotaFiscalByChave(chave);
            return notaDb.LoadXmlAsync();
        }

        private async void LoadedCmd_Execute()
        {
            _isLoaded = true;

            if (!string.IsNullOrEmpty(_mensagensErroContingencia))
            {
                var app = Application.Current;
                var mainWindow = app.MainWindow;
                MessageBox.Show(mainWindow,
                    _mensagensErroContingencia +
                    "Ocorreram os seguintes erros ao transmitir as notas em contingência:\n", "Erro contingência", MessageBoxButton.OK, MessageBoxImage.Information);
                _mensagensErroContingencia = null;
            }

            try
            {
                Task popularListaNfTask = PopularListaNotasFiscaisAsync();
                Task<List<NotaFiscalEntity>> notasFiscaisPendentesTask = _notaFiscalRepository.GetNotasPendentesAsync(false);

                var certificado = _certificadoService.GetX509Certificate2();
                var config = _configuracaoService.GetConfiguracao();
                var codigoUf = UfToCodigoUfConversor.GetCodigoUf(_emissorService.GetEmissor().Endereco.UF);

                await popularListaNfTask;
                var notasFiscaisPendentes = await notasFiscaisPendentesTask;
                await AtualizarNotasPendentesAsync(certificado, notasFiscaisPendentes, codigoUf);
            }
            catch (Exception e)
            {
                log.Error(e);
                var x509Certificate2 = _certificadoService.GetX509Certificate2();

                if (x509Certificate2.NotAfter < DateTime.Now) MessageBox.Show("Certificado vencido.", "Erro.");
            }
        }

        private void MudarPaginaCmd_Execute(int page)
        {
            PopularListaNotasFiscaisAsync(page);
        }

        private async void ModoOnlineService_NotasTransmitidasEventHandler(List<string> mensagensErro)
        {
            if (_isLoaded)
            {
                await PopularListaNotasFiscaisAsync();
                await Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(async () =>
                {
                    var certificado = _certificadoService.GetX509Certificate2();
                    var config = await _configuracaoService.GetConfiguracaoAsync();
                    var notasFiscaisPendentes = _notaFiscalRepository.GetNotasPendentes(false);
                    var codigoUf = UfToCodigoUfConversor.GetCodigoUf(_emissorService.GetEmissor().Endereco.UF);
                    await AtualizarNotasPendentesAsync(certificado, notasFiscaisPendentes, codigoUf);
                }));
            }

            if (mensagensErro.Count == 0)
                return;

            await Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                var app = Application.Current;
                var mainWindow = app.MainWindow;
                var sb = new StringBuilder();

                foreach (var msg in mensagensErro)
                    sb.Append("\n" + msg);

                if (_isLoaded)
                    MessageBox.Show(mainWindow, "Ocorreram os seguintes erros ao transmitir as notas em contingência:\n" +
                        sb, "Erro contingência", MessageBoxButton.OK, MessageBoxImage.Information);
                else
                    _mensagensErroContingencia = sb.ToString();
            }));
        }

        private void NotaCanceladaVM_NotaInutilizadaEventHandler(Chave chave)
        {
            var notaMemento = NotasFiscais.First(n => n.Chave == chave.ToString());
            NotasFiscais.Remove(notaMemento);
        }

        private void NotaFiscalVM_NotaCanceladaEventHandler(NotaFiscalEntity nota)
        {
            var notaCancelada = NotasFiscais.FirstOrDefault(n => n.Chave == nota.Chave);
            var index = NotasFiscais.IndexOf(notaCancelada);

            var notaMemento = new NotaFiscalMemento(nota.Numero,
                nota.Modelo == "NFC-e" ? Modelo.Modelo65 : Modelo.Modelo55, nota.DataEmissao, nota.DataAutorizacao,
                nota.Destinatario, nota.UfDestinatario, nota.ValorTotal.ToString("N2", new CultureInfo("pt-BR")),
                new StatusEnvio((Status)nota.Status).ToString(), nota.Chave);

            NotasFiscais[index] = notaMemento;
        }

        private void OpcoesVM_ConfiguracaoAlteradaEventHandler()
        {
            PopularListaNotasFiscaisAsync();
        }

        private Task PopularListaNotasFiscaisAsync(int page = 1)
        {
            return Task.Run(async () =>
            {
                var notasDb = await _notaFiscalRepository.TakeAsync(100, page);

                if (notasDb == null) return;

                var notaFiscalMementos = notasDb
                    .AsParallel()
                    .Select(nota => new NotaFiscalMemento(nota.Numero,
                        nota.Modelo == "65" ? Modelo.Modelo65 : Modelo.Modelo55, nota.DataEmissao, nota.DataAutorizacao,
                        nota.Destinatario, nota.UfDestinatario,
                        nota.ValorTotal.ToString("N2", new CultureInfo("pt-BR")), new StatusEnvio((Status)nota.Status).ToString(), nota.Chave))
                    .OrderByDescending(n => n.DataEmissão);

                await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    NotasFiscais = new ObservableCollection<NotaFiscalMemento>(notaFiscalMementos);
                }));
            });
        }

        private void SubscribeToEvents()
        {
            MessagingCenter.Subscribe<EnviarNotaAppService, NotaFiscalEnviadaEvent>(this, nameof(NotaFiscalEnviadaEvent),
                (s, e) => { EnviarNotaController_NotaEnviadaEventHandler(); });

            MessagingCenter.Subscribe<ModoOnlineService, NotasFiscaisTransmitidasEvent>(this,
                nameof(NotasFiscaisTransmitidasEvent),
                (s, e) => { ModoOnlineService_NotasTransmitidasEventHandler(e.MensagensErro); });

            MessagingCenter.Subscribe<OpcoesViewModel, ConfiguracaoAlteradaEvent>(this, nameof(ConfiguracaoAlteradaEvent),
                (s, e) => { OpcoesVM_ConfiguracaoAlteradaEventHandler(); });

            MessagingCenter.Subscribe<CancelarNotaViewModel, NotaFiscalCanceladaEvent>(this, nameof(NotaFiscalCanceladaEvent),
                (s, e) => { NotaFiscalVM_NotaCanceladaEventHandler(e.NotaFiscal); });

            MessagingCenter.Subscribe<CancelarNotaViewModel, NotaFiscalInutilizadaEvent>(this,
                nameof(NotaFiscalInutilizadaEvent), (s, e) => { NotaCanceladaVM_NotaInutilizadaEventHandler(e.Chave); });
        }

        private async void VisualizarNotaCmd_ExecuteAsync(NotaFiscalMemento notaFiscalMemento)
        {
            string xml = await GetNotaXmlAsync(notaFiscalMemento.Chave);
            var notaFiscalDto = _notaFiscalRepository.GetNotaFiscalFromNfeProcXml(xml);

            notaFiscalDto.QrCodeUrl = xml;
            _visualizarNotaEnviadaViewModel.VisualizarNotaFiscal(notaFiscalDto);
        }
    }
}