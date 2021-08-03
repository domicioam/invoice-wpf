using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFe.Core.NotasFiscais
{
    // Domain Service
    public class NotaFiscalService
    {
        public Numeracao GerarNumeraçãoPróximaNotaFiscal(Domain.Modelo modelo)
        {
            // must be atomic operation get number and increase in one transaction


            throw new NotImplementedException();
        }
    }
}
