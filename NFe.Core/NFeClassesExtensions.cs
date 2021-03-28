using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NFe.Core.XmlSchemas.NfeAutorizacao.Envio;

namespace NFe.Core.Sefaz
{
    static class NFeClassesExtensions
    {
        public static XmlSchemas.NfeAutorizacao.Retorno.NfeProc.TNFe ToTNFeRetorno(this TNFe nfe, string nfeNamespaceName)
        {
            var nfeSerializada = XmlUtil.Serialize(nfe, nfeNamespaceName);
            return (XmlSchemas.NfeAutorizacao.Retorno.NfeProc.TNFe)XmlUtil.Deserialize<XmlSchemas.NfeAutorizacao.Retorno.NfeProc.TNFe>(nfeSerializada);
        }
    }
}
