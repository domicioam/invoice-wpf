using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NFe.Core.NFeAutorizacao4;
using System.Net;
using System.ServiceModel;

namespace NFe.Core.UnitTests.NotaFiscalService
{
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public class NFeAutorizacao4SoapClientFake : ClientBase<NFe.Core.NFeAutorizacao4.NFeAutorizacao4Soap>, NFe.Core.NFeAutorizacao4.NFeAutorizacao4Soap
    {

        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        NFe.Core.NFeAutorizacao4.nfeAutorizacaoLoteResponse NFe.Core.NFeAutorizacao4.NFeAutorizacao4Soap.nfeAutorizacaoLote(NFe.Core.NFeAutorizacao4.nfeAutorizacaoLoteRequest request)
        {
            return base.Channel.nfeAutorizacaoLote(request);
        }

        public System.Xml.XmlNode nfeAutorizacaoLote(System.Xml.XmlNode nfeDadosMsg)
        {
            throw new WebException();
        }

        public Task<nfeAutorizacaoLoteResponse> nfeAutorizacaoLoteAsync(nfeAutorizacaoLoteRequest request)
        {
            throw new NotImplementedException();
        }

        public nfeAutorizacaoLoteZipResponse nfeAutorizacaoLoteZip(nfeAutorizacaoLoteZipRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<nfeAutorizacaoLoteZipResponse> nfeAutorizacaoLoteZipAsync(nfeAutorizacaoLoteZipRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
