using DgSystem.NFe.Reports;
using MediatR;
using NFe.Core;
using NFe.Core.Utils.PDF;
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

            // if nfce
            // call DanfeNfce


            var notaFiscal = new DgSystem.NFe.Reports.NotaFiscal();


            if(request.NotaFiscal.Identificacao.Modelo == global::NFe.Core.Domain.Modelo.Modelo65)
            {
                DanfeNfce.GerarPDFNfce()
            }


            return GeradorPDF.GerarPdfNotaFiscal(request.NotaFiscal);
        }
    }
}
