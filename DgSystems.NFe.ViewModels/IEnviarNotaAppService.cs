using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Views;
using NFe.Core.NotasFiscais;
using NFe.WPF.Model;
using NFe.WPF.NotaFiscal.Model;
using NFe.WPF.ViewModel;

namespace NFe.WPF.NotaFiscal.ViewModel
{
    public interface IEnviarNotaAppService
    {
        Task<Core.NotasFiscais.NotaFiscal> EnviarNotaAsync(NotaFiscalModel notaFiscalModel, Modelo _modelo, Core.NotasFiscais.Emissor emissor, X509Certificate2 certificado, IDialogService dialogService);
        Task ImprimirNotaFiscal(Core.NotasFiscais.NotaFiscal notaFiscal);
    }
}