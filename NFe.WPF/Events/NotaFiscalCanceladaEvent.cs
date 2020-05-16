using MediatR;
using NFe.Core.Entitities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFe.WPF.Events
{
    public class NotaFiscalCanceladaEvent : INotification
    {
        public NotaFiscalEntity NotaFiscal { get; internal set; }
    }
}
