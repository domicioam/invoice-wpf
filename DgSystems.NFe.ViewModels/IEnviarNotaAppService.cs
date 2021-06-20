using GalaSoft.MvvmLight.Views;
using NFe.Core.Domain;
using NFe.WPF.NotaFiscal.Model;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace NFe.WPF.NotaFiscal.ViewModel
{
    public interface IEnviarNotaAppService
    {
        Task<NFe.Core.Domain.NotaFiscal> EnviarNotaAsync(NotaFiscalModel notaFiscalModel, Modelo _modelo, Emissor emissor, X509Certificate2 certificado, IDialogService dialogService);
        void ImprimirNotaFiscal(Core.Domain.NotaFiscal notaFiscal);
    }
}