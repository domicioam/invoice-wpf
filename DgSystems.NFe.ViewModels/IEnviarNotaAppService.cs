using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Views;
using NFe.Core.Domain;
using NFe.WPF.Model;
using NFe.WPF.NotaFiscal.Model;
using NFe.WPF.ViewModel;

namespace NFe.WPF.NotaFiscal.ViewModel
{
    public interface IEnviarNotaAppService
    {
        Task<NFe.Core.Domain.NotaFiscal> EnviarNotaAsync(NotaFiscalModel notaFiscalModel, Modelo _modelo, NFe.Core.Domain.Emissor emissor, X509Certificate2 certificado, IDialogService dialogService);
        Task ImprimirNotaFiscal(NFe.Core.Domain.NotaFiscal notaFiscal);
    }
}