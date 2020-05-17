using System.Threading.Tasks;
using NFe.Core.NotasFiscais;
using NFe.WPF.Model;
using NFe.WPF.ViewModel;

namespace NFe.WPF.NotaFiscal.ViewModel
{
    public interface IEnviarNota
    {
        Task<Core.NotasFiscais.NotaFiscal> EnviarNota(NotaFiscalModel notaFiscalModel, Modelo _modelo);
        Task ImprimirNotaFiscal(Core.NotasFiscais.NotaFiscal notaFiscal);
    }
}