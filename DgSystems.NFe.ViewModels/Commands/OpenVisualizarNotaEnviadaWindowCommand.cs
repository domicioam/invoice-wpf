using NFe.WPF.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DgSystems.NFe.ViewModels.Commands
{
    public class OpenVisualizarNotaEnviadaWindowCommand
    {
        public VisualizarNotaEnviadaViewModel VisualizarNotaEnviadaViewModel { get; set; }

        public OpenVisualizarNotaEnviadaWindowCommand(VisualizarNotaEnviadaViewModel viewModel)
        {
            VisualizarNotaEnviadaViewModel = viewModel;
        }
    }
}
