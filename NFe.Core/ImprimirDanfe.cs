using MediatR;
using NFe.Core.Domain;

namespace NFe.Core
{
    public class ImprimirDanfe : IRequest<bool>
    {
        public ImprimirDanfe(NotaFiscal notaFiscal)
        {
            NotaFiscal = notaFiscal;
        }

        public NotaFiscal NotaFiscal { get; }
    }
}