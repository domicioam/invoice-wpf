using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DgSystems.NFe.Services
{
    public class ConsultaReciboException : Exception
    {
        public ConsultaReciboException() : base()
        {
        }

        public ConsultaReciboException(string message) : base(message)
        {
        }

        public ConsultaReciboException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
