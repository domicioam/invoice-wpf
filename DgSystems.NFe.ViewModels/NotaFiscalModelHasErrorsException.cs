using System;
using System.Runtime.Serialization;

namespace NFe.WPF.NotaFiscal.ViewModel
{
    [Serializable]
    public class NotaFiscalModelHasErrorsException : ArgumentException
    {
        public NotaFiscalModelHasErrorsException()
        {
        }

        public NotaFiscalModelHasErrorsException(string message) : base(message)
        {
        }

        public NotaFiscalModelHasErrorsException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NotaFiscalModelHasErrorsException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}