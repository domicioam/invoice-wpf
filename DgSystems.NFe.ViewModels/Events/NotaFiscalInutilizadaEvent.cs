using EmissorNFe.Model;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NFe.WPF.NotaFiscal.Model;
using DgSystems.NFe.ViewModels;
using NFe.Core.NotaFiscal;

namespace NFe.WPF.Events
{
    public class NotaFiscalInutilizadaEvent 
    {
        public Chave Chave { get; internal set; }
    }
}
