using MediatR;

namespace NFe.Core.Domain
{
    public class GerarDanfeNfceEmail : IRequest<string>
    {
        public GerarDanfeNfceEmail(NotaFiscal notaFiscal)
        {
            NotaFiscal = notaFiscal;
        }

        public NotaFiscal NotaFiscal { get; }
    }
}
