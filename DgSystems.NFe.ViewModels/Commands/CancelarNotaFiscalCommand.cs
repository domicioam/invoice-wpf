using NFe.WPF.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DgSystems.NFe.ViewModels.Commands
{
    public class CancelarNotaFiscalCommand
    {
        public CancelarNotaViewModel CancelarNotaViewModel { get; set; }

        public CancelarNotaFiscalCommand(CancelarNotaViewModel viewModel)
        {
            CancelarNotaViewModel = viewModel;
        }
    }
}
