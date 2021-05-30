using MediatR;
using NFe.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DgSystems.NFe.NotaFiscal.Reports
{
    public class ImprimirDanfeHandler : IRequestHandler<ImprimirDanfe, bool>
    {
        // send message from NotaFiscal bounded context to Reports bounded context
        public Task<bool> Handle(ImprimirDanfe request, CancellationToken cancellationToken)
        {
            // manda imprimir e responde com true se sucesso

            throw new NotImplementedException();
        }
    }
}
