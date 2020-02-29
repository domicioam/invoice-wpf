using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFe.WPF.Exceptions
{
    public class NotaFiscalModelHasErrorsException : ArgumentException
    {
        public NotaFiscalModelHasErrorsException(string message) : base(message)
        {
        }
    }
}
