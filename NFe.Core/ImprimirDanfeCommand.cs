using NFe.Core.Domain;
using NFe.Core.Utils.PDF;

namespace NFe.Core
{
    public class ImprimirDanfeCommand : Command
    {
        public ImprimirDanfeCommand(NotaFiscal notaFiscal)
        {
            NotaFiscal = notaFiscal;
        }

        public NotaFiscal NotaFiscal { get; }

        public override async void Execute()
        {
            try
            {
                await GeradorPDF.GerarPdfNotaFiscal(NotaFiscal);
                IsExecuted = true;
            }
            catch
            {
                IsExecuted = false;
            }
        }
    }
}
