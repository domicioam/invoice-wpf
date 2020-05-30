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

            container.Register<MainViewModel>(Lifestyle.Singleton);
            container.Register<ImpostoViewModel>(Lifestyle.Singleton);
            container.Register<ProdutoViewModel>(Lifestyle.Singleton);
            container.Register<EmitenteViewModel>(Lifestyle.Singleton);
            container.Register<OpcoesViewModel>(Lifestyle.Singleton);
            container.Register<CertificadoViewModel>(Lifestyle.Singleton);
            container.Register<NFCeViewModel>(Lifestyle.Singleton);
            container.Register<NotaFiscalMainViewModel>(Lifestyle.Singleton);
            container.Register<NFeViewModel>(Lifestyle.Singleton);
            container.Register<DestinatarioMainViewModel>(Lifestyle.Singleton);
            container.Register<ProdutoMainViewModel>(Lifestyle.Singleton);
            container.Register<ImpostoMainViewModel>(Lifestyle.Singleton);
            container.Register<DestinatarioViewModel>(Lifestyle.Singleton);
            container.Register<CancelarNotaViewModel>(Lifestyle.Singleton);
            container.Register<VisualizarNotaEnviadaViewModel>(Lifestyle.Singleton);
            container.Register<EnviarContabilidadeViewModel>(Lifestyle.Singleton);
            container.Register<AcompanhamentoNotasViewModel>(Lifestyle.Singleton);
            container.Register<EnviarEmailViewModel>(Lifestyle.Singleton);
            container.Register<ImportarXMLViewModel>(Lifestyle.Singleton);

            container.Register<IDialogService, MessageService>(Lifestyle.Singleton);

            container.Register<INFeConsulta, NFeConsulta>(Lifestyle.Singleton);
            container.Register<ICertificateManager, CertificateManager>(Lifestyle.Singleton);
            container.Register<IEnviarNota, EnviarNotaController>(Lifestyle.Singleton);
            container.Register<IEnviaNotaFiscalFacade, EnviaNotaFiscalFacade>(Lifestyle.Singleton);
            container.Register<IServiceFactory, ServiceFactory>(Lifestyle.Singleton);
            container.Register<INotaInutilizadaService, NotaInutilizadaService>(Lifestyle.Singleton);
            container.Register<IEventoService, EventoService>(Lifestyle.Singleton);
            container.Register<IMunicipioService, MunicipioService>(Lifestyle.Singleton);
            container.Register<ImportadorXmlService>(Lifestyle.Singleton);
            container.Register<IEmissorService, EmissorService>(Lifestyle.Singleton);
            container.Register<ICertificadoService, CertificadoService>(Lifestyle.Singleton);
            container.Register<IConfiguracaoService, ConfiguracaoService>(Lifestyle.Singleton);
            container.Register<IConsultaStatusServicoFacade, ConsultaStatusServicoFacade>(Lifestyle.Singleton);
            container.Register<IEventoRepository, EventoRepository>(Lifestyle.Singleton);
            container.Register<INotaFiscalRepository, NotaFiscalRepository>(Lifestyle.Singleton);
            container.Register<ICertificadoRepository, CertificadoRepository>(Lifestyle.Singleton);
            container.Register<INotaInutilizadaRepository, NotaInutilizadaRepository>(Lifestyle.Singleton);
            container.Register<IConfiguracaoRepository, ConfiguracaoRepository>(Lifestyle.Singleton);
            container.Register<IDestinatarioRepository, DestinatarioRepository>(Lifestyle.Singleton);
            container.Register<IEmitenteRepository, EmitenteRepository>(Lifestyle.Singleton);
            container.Register<IGrupoImpostosRepository, GrupoImpostosRepository>(Lifestyle.Singleton);
            container.Register<IProdutoRepository, ProdutoRepository>(Lifestyle.Singleton);
            container.Register<ITransportadoraRepository, TransportadoraRepository>(Lifestyle.Singleton);
            container.Register<IEstadoRepository, EstadoRepository>(Lifestyle.Singleton);
            container.Register<IMunicipioRepository, MunicipioRepository>(Lifestyle.Singleton);
            container.Register<INaturezaOperacaoRepository, NaturezaOperacaoRepository>(Lifestyle.Singleton);
            container.Register<IHistoricoEnvioContabilidadeRepository, HistoricoEnvioContabilidadeRepository>(Lifestyle.Singleton);
            container.Register<IEmiteNotaFiscalContingenciaFacade, EmiteEmiteNotaFiscalContingenciaFacade>(Lifestyle.Singleton);
            container.Register<ICancelaNotaFiscalFacade, CancelaNotaFiscalFacade>(Lifestyle.Singleton);
            container.Register<NFeInutilizacao>(Lifestyle.Singleton);
            container.Register<INFeCancelamento, NFeCancelamento>(Lifestyle.Singleton);
            container.Register<MailManager>(Lifestyle.Singleton);
            container.Register<ModoOnlineService>(Lifestyle.Singleton);
            container.Register<IDestinatarioService, DestinatarioService>(Lifestyle.Singleton);
            container.Register<INaturezaOperacaoService, NaturezaOperacaoService>(Lifestyle.Singleton);
            container.Register<GeradorZip>(Lifestyle.Singleton);
            container.Register<GeradorPDF>(Lifestyle.Singleton);
            container.Register<ITransportadoraService, TransportadoraService>(Lifestyle.Singleton);
            container.Register<SefazSettings>(Lifestyle.Singleton);
            container.Register<InutilizarNotaFiscalFacade>(Lifestyle.Singleton); 

            container.Verify();

            DependencyResolver = container;
        }

        public static Container DependencyResolver { get; set; }
    }
}
