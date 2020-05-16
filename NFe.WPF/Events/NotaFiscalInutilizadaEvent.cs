using EmissorNFe.Model;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFe.WPF.Events
{
    public class NotaFiscalInutilizadaEvent : IRequest
    {
        public NFCeModel NotaFiscal { get; internal set; }
    }
}
