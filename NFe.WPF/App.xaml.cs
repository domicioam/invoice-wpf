using EmissorNFe.ViewModel;
using Microsoft.Practices.ServiceLocation;
using NFe.Core;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using GalaSoft.MvvmLight.Views;
using NFe.Core.Cadastro;
using NFe.Core.Cadastro.Certificado;
using NFe.Core.Cadastro.Configuracoes;
using NFe.Core.Cadastro.Destinatario;
using NFe.Core.Cadastro.Emissor;
using NFe.Core.Cadastro.Imposto;
using NFe.Core.Cadastro.Transportadora;
using NFe.Core.Interfaces;
using NFe.Core.NotasFiscais.Sefaz.NfeConsulta2;
using NFe.Core.NotasFiscais.Sefaz.NfeInutilizacao2;
using NFe.Core.NotasFiscais.Sefaz.NfeRecepcaoEvento;
using NFe.Core.NotasFiscais.Services;
using NFe.Core.Utils;
using NFe.Core.Utils.PDF;
using NFe.Core.Utils.Zip;
using NFe.Repository;
using NFe.Repository.Repositories;
using NFe.WPF.View;
using NFe.WPF.ViewModel;
using NFe.WPF.ViewModel.Services;
using SimpleInjector;
using System.Reflection;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;
using NFe.Core.NotasFiscais;
using NFe.Core.Utils.Assinatura;
using NFe.WPF.NotaFiscal.ViewModel;
using NFe.WPF.Utils;
using NFe.Core.Sefaz;
using NFe.Core.Sefaz.Facades;
using MediatR;

using MediatR.Pipeline;
using MediatR.SimpleInjector;
using NFe.Core.Events;
using NFe.WPF.Events;
using NFe.WPF.Commands;

namespace EmissorNFe
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Application Entry Point.
        /// </summary>
        [System.STAThreadAttribute()]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public static void Main()
        {
            SplashScreen splashScreen = new SplashScreen("view/splashscreen.png");
            splashScreen.Show(true);
            EmissorNFe.App app = new EmissorNFe.App();
            Run(app);
        }

        /// <summary>
        /// Execute a form base application if another instance already running on
        /// the system activate previous one
        /// </summary>
        /// <param name="frmMain">main form</param>
        /// <returns>true if no previous instance is running</returns>
        public static bool Run(EmissorNFe.App app)
        {
            if (IsAlreadyRunning())
            {
                //set focus on previously running app
                SwitchToCurrentInstance();
                return false;
            }
            app.InitializeComponent();
            app.Run();
            return true;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            CreateDataDirectory();
            RegisterTypes();
            base.OnStartup(e);
        }

        private static Task CreateDataDirectory()
        {
            return Task.Run(() =>
            {
                string appDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Notas Fiscais\Data");

                if (!Directory.Exists(appDataDir))
                {
                    Directory.CreateDirectory(appDataDir);
                }

                AppDomain.CurrentDomain.SetData("DataDirectory", appDataDir);
            });
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // put your tracing or logging code here (I put a message box as an example)
            MessageBox.Show(e.ExceptionObject.ToString());

            string sDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "EmissorNFeDir");

            if (!Directory.Exists(sDirectory))
            {
                Directory.CreateDirectory(sDirectory);
            }

            using (FileStream stream = File.Create(Path.Combine(sDirectory, "log.txt")))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.WriteLine(e.ExceptionObject.ToString());
                }
            }
        }

        /// <summary>
        /// check if given exe alread running or not
        /// </summary>
        /// <returns>returns true if already running</returns>
        private static bool IsAlreadyRunning()
        {
            string strLoc = Assembly.GetExecutingAssembly().Location;
            FileSystemInfo fileInfo = new FileInfo(strLoc);
            string sExeName = fileInfo.Name;
            bool bCreatedNew;

            mutex = new Mutex(true, "Global\\" + sExeName, out bCreatedNew);
            if (bCreatedNew)
                mutex.ReleaseMutex();

            return !bCreatedNew;
        }

        static Mutex mutex;
        const int SW_RESTORE = 9;


        /// <summary>
        /// Imports 
        /// </summary>

        [DllImport("user32.dll")]
        private static extern int ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern int SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int IsIconic(IntPtr hWnd);

        /// <summary>
        /// GetCurrentInstanceWindowHandle
        /// </summary>
        /// <returns></returns>
        private static IntPtr GetCurrentInstanceWindowHandle()
        {
            IntPtr hWnd = IntPtr.Zero;
            Process process = Process.GetCurrentProcess();
            Process[] processes = Process.GetProcessesByName(process.ProcessName);
            foreach (Process _process in processes)
            {
                // Get the first instance that is not this instance, has the
                // same process name and was started from the same file name
                // and location. Also check that the process has a valid
                // window handle in this session to filter out other user's
                // processes.
                if (_process.Id != process.Id &&
                    _process.MainModule.FileName == process.MainModule.FileName &&
                    _process.MainWindowHandle != IntPtr.Zero)
                {
                    hWnd = _process.MainWindowHandle;
                    break;
                }
            }
            return hWnd;
        }
        /// <summary>
        /// SwitchToCurrentInstance
        /// </summary>
        private static void SwitchToCurrentInstance()
        {
            IntPtr hWnd = GetCurrentInstanceWindowHandle(); //não funciona
            if (hWnd != IntPtr.Zero)
            {
                // Restore window if minimised. Do not restore if already in
                // normal or maximised window state, since we don't want to
                // change the current state of the window.
                if (IsIconic(hWnd) != 0)
                {
                    ShowWindow(hWnd, SW_RESTORE);
                }

                // Set foreground window.
                SetForegroundWindow(hWnd);
            }
        }

        private void RegisterTypes()
        {
            var container = new Container();

            container.Register<MainViewModel>();
            container.Register<ImpostoViewModel>();
            container.Register<ProdutoViewModel>();
            container.Register<EmitenteViewModel>();
            container.Register<OpcoesViewModel>();
            container.Register<CertificadoViewModel>();
            container.Register<NFCeViewModel>();
            container.Register<NotaFiscalMainViewModel>();
            container.Register<NFeViewModel>();
            container.Register<DestinatarioMainViewModel>();
            container.Register<ProdutoMainViewModel>();
            container.Register<ImpostoMainViewModel>();
            container.Register<DestinatarioViewModel>();
            container.Register<CancelarNotaViewModel>();
            container.Register<VisualizarNotaEnviadaViewModel>();
            container.Register<EnviarContabilidadeViewModel>();
            container.Register<AcompanhamentoNotasViewModel>();
            container.Register<EnviarEmailViewModel>();
            container.Register<ImportarXMLViewModel>();

            container.Register<IDialogService, MessageService>();

            container.Register<INFeConsulta, NFeConsulta>();
            container.Register<ICertificateManager, CertificateManager>();
            container.Register<IEnviarNota, EnviarNotaController>();
            container.Register<IEnviaNotaFiscalFacade, EnviaNotaFiscalFacade>();
            container.Register<IServiceFactory, NFe.Core.NotasFiscais.ServiceFactory>();
            container.Register<INotaInutilizadaService, NotaInutilizadaService>();
            container.Register<IEventoService, EventoService>();
            container.Register<IMunicipioService, MunicipioService>();
            container.Register<ImportadorXmlService>();
            container.Register<IEmissorService, EmissorService>();
            container.Register<ICertificadoService, CertificadoService>();
            container.Register<IConfiguracaoService, ConfiguracaoService>();
            container.Register<IConsultaStatusServicoFacade, ConsultaStatusServicoFacade>();
            container.Register<IEventoRepository, EventoRepository>();
            container.Register<INotaFiscalRepository, NotaFiscalRepository>();
            container.Register<ICertificadoRepository, CertificadoRepository>();
            container.Register<INotaInutilizadaRepository, NotaInutilizadaRepository>();
            container.Register<IConfiguracaoRepository, ConfiguracaoRepository>();
            container.Register<IDestinatarioRepository, DestinatarioRepository>();
            container.Register<IEmitenteRepository, EmitenteRepository>();
            container.Register<IGrupoImpostosRepository, GrupoImpostosRepository>();
            container.Register<IProdutoRepository, ProdutoRepository>();
            container.Register<ITransportadoraRepository, TransportadoraRepository>();
            container.Register<IEstadoRepository, EstadoRepository>();
            container.Register<IMunicipioRepository, MunicipioRepository>();
            container.Register<INaturezaOperacaoRepository, NaturezaOperacaoRepository>();
            container.Register<IHistoricoEnvioContabilidadeRepository, HistoricoEnvioContabilidadeRepository>();
            container.Register<IEmiteNotaFiscalContingenciaFacade, EmiteEmiteNotaFiscalContingenciaFacade>();
            container.Register<ICancelaNotaFiscalFacade, CancelaNotaFiscalFacade>();
            container.Register<NFeInutilizacao>();
            container.Register<INFeCancelamento, NFeCancelamento>();
            container.Register<MailManager>();
            container.Register<ModoOnlineService>();
            container.Register<IEstadoService, EstadoService>();
            container.Register<IDestinatarioService, DestinatarioService>();
            container.Register<INaturezaOperacaoService, NaturezaOperacaoService>();
            container.Register<GeradorZip>();
            container.Register<GeradorPDF>();
            container.Register<ITransportadoraService, TransportadoraService>();
            container.Register<SefazSettings>();
            container.Register<InutilizarNotaFiscalFacade>();
            // Event and Commands
            container.Register<IRequestHandler<NotaFiscalEmitidaEmContingenciaEvent, Unit>, ModoOnlineService>();
            container.Collection.Register<INotificationHandler<NotaFiscalEnviadaEvent>>(typeof(AcompanhamentoNotasViewModel), typeof(NotaFiscalMainViewModel));
            container.Collection.Register<INotificationHandler<NotaFiscalCanceladaEvent>>(typeof(AcompanhamentoNotasViewModel), typeof(NotaFiscalMainViewModel));
            container.Collection.Register<INotificationHandler<ConfiguracaoAlteradaEvent>>(typeof(AcompanhamentoNotasViewModel), typeof(NotaFiscalMainViewModel));
            container.Register<IRequestHandler<NotaFiscalPendenteReenviadaEvent, Unit>, AcompanhamentoNotasViewModel>();
            container.Register<IRequestHandler<ImpostoAdicionadoEvent, Unit>, ImpostoMainViewModel>();
            container.Collection.Register<INotificationHandler<DestinatarioSalvoEvent>>(typeof(NFCeViewModel), typeof(NFeViewModel), typeof(DestinatarioMainViewModel));
            container.Register<IRequestHandler<NotaFiscalInutilizadaEvent, Unit>, NotaFiscalMainViewModel>();
            container.Register<IRequestHandler<NotasFiscaisTransmitidasEvent, Unit>, NotaFiscalMainViewModel>();
            container.Register<IRequestHandler<VisualizarNotaFiscalCommand, Unit>, VisualizarNotaEnviadaViewModel>();
            container.Register<IRequestHandler<ProdutoAdicionadoEvent, Unit>, ProdutoMainViewModel>();
            container.Register<IRequestHandler<AlterarDestinatarioCommand, Unit>, DestinatarioViewModel>();
            container.Register<IRequestHandler<EnviarEmailCommand, Unit>, EnviarEmailViewModel>();

            container.BuildMediator();

            container.Verify();

            DependencyResolver = container;
        }

        public static Container DependencyResolver { get; set; }
    }
}
