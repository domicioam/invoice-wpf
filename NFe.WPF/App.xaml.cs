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

            container.Register<MainViewModel>(Lifestyle.Transient);
            container.Register<ImpostoViewModel>(Lifestyle.Transient);
            container.Register<ProdutoViewModel>(Lifestyle.Transient);
            container.Register<EmitenteViewModel>(Lifestyle.Transient);
            container.Register<OpcoesViewModel>(Lifestyle.Transient);
            container.Register<CertificadoViewModel>(Lifestyle.Transient);
            container.Register<NFCeViewModel>(Lifestyle.Transient);
            container.Register<NotaFiscalMainViewModel>(Lifestyle.Transient);
            container.Register<NFeViewModel>(Lifestyle.Transient);
            container.Register<DestinatarioMainViewModel>(Lifestyle.Transient);
            container.Register<ProdutoMainViewModel>(Lifestyle.Transient);
            container.Register<ImpostoMainViewModel>(Lifestyle.Transient);
            container.Register<DestinatarioViewModel>(Lifestyle.Transient);
            container.Register<CancelarNotaViewModel>(Lifestyle.Transient);
            container.Register<VisualizarNotaEnviadaViewModel>(Lifestyle.Transient);
            container.Register<EnviarContabilidadeViewModel>(Lifestyle.Transient);
            container.Register<AcompanhamentoNotasViewModel>(Lifestyle.Transient);
            container.Register<EnviarEmailViewModel>(Lifestyle.Transient);
            container.Register<ImportarXMLViewModel>(Lifestyle.Transient);

            container.Register<IDialogService, MessageService>(Lifestyle.Transient);

            container.Register<INFeConsulta, NFeConsulta>(Lifestyle.Transient);
            container.Register<ICertificateManager, CertificateManager>(Lifestyle.Transient);
            container.Register<IEnviarNota, EnviarNotaController>(Lifestyle.Transient);
            container.Register<IEnviaNotaFiscalFacade, EnviaNotaFiscalFacade>(Lifestyle.Transient);
            container.Register<IServiceFactory, ServiceFactory>(Lifestyle.Transient);
            container.Register<INotaInutilizadaService, NotaInutilizadaService>(Lifestyle.Transient);
            container.Register<IEventoService, EventoService>(Lifestyle.Transient);
            container.Register<IMunicipioService, MunicipioService>(Lifestyle.Transient);
            container.Register<ImportadorXmlService>(Lifestyle.Transient);
            container.Register<IEmissorService, EmissorService>(Lifestyle.Transient);
            container.Register<ICertificadoService, CertificadoService>(Lifestyle.Transient);
            container.Register<IConfiguracaoService, ConfiguracaoService>(Lifestyle.Transient);
            container.Register<IConsultaStatusServicoFacade, ConsultaStatusServicoFacade>(Lifestyle.Transient);
            container.Register<IEventoRepository, EventoRepository>(Lifestyle.Transient);
            container.Register<INotaFiscalRepository, NotaFiscalRepository>(Lifestyle.Transient);
            container.Register<ICertificadoRepository, CertificadoRepository>(Lifestyle.Transient);
            container.Register<INotaInutilizadaRepository, NotaInutilizadaRepository>(Lifestyle.Transient);
            container.Register<IConfiguracaoRepository, ConfiguracaoRepository>(Lifestyle.Transient);
            container.Register<IDestinatarioRepository, DestinatarioRepository>(Lifestyle.Transient);
            container.Register<IEmitenteRepository, EmitenteRepository>(Lifestyle.Transient);
            container.Register<IGrupoImpostosRepository, GrupoImpostosRepository>(Lifestyle.Transient);
            container.Register<IProdutoRepository, ProdutoRepository>(Lifestyle.Transient);
            container.Register<ITransportadoraRepository, TransportadoraRepository>(Lifestyle.Transient);
            container.Register<IEstadoRepository, EstadoRepository>(Lifestyle.Transient);
            container.Register<IMunicipioRepository, MunicipioRepository>(Lifestyle.Transient);
            container.Register<INaturezaOperacaoRepository, NaturezaOperacaoRepository>(Lifestyle.Transient);
            container.Register<IHistoricoEnvioContabilidadeRepository, HistoricoEnvioContabilidadeRepository>(Lifestyle.Transient);
            container.Register<IEmiteNotaFiscalContingenciaFacade, EmiteEmiteNotaFiscalContingenciaFacade>(Lifestyle.Transient);
            container.Register<ICancelaNotaFiscalFacade, CancelaNotaFiscalFacade>(Lifestyle.Transient);
            container.Register<NFeInutilizacao>(Lifestyle.Transient);
            container.Register<INFeCancelamento, NFeCancelamento>(Lifestyle.Transient);
            container.Register<MailManager>(Lifestyle.Transient);
            container.Register<ModoOnlineService>(Lifestyle.Transient);
            container.Register<IEstadoService, EstadoService>(Lifestyle.Transient);
            container.Register<IDestinatarioService, DestinatarioService>(Lifestyle.Transient);
            container.Register<INaturezaOperacaoService, NaturezaOperacaoService>(Lifestyle.Transient);
            container.Register<GeradorZip>(Lifestyle.Transient);
            container.Register<GeradorPDF>(Lifestyle.Transient);
            container.Register<ITransportadoraService, TransportadoraService>(Lifestyle.Transient);
            container.Register<SefazSettings>(Lifestyle.Transient);

            container.Verify();

            DependencyResolver = container;
        }

        public static Container DependencyResolver { get; set; }
    }
}
