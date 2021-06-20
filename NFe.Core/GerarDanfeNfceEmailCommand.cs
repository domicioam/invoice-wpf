using MediatR;
using NFe.Core.Domain;

namespace NFe.Core
{
    public class GerarDanfeNfceEmailCommand : Command
    {
        private readonly IMediator mediator;

        public GerarDanfeNfceEmailCommand(NotaFiscal notaFiscal, IMediator mediator)
        {
            NotaFiscal = notaFiscal;
            this.mediator = mediator;
        }

        public NotaFiscal NotaFiscal { get; }
        public string Result { get; private set; }

        public override async void ExecuteAsync()
        {
            try
            {
                string result = await mediator.Send(new GerarDanfeNfceEmail(NotaFiscal));
                IsExecuted = result != string.Empty;
                Result = result;
            }
            catch
            {
                IsExecuted = false;
            }
        }
    }
}
