using NFe.WPF.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DgSystems.NFe.ViewModels.Commands
{
    public class OpenEnviarEmailWindowCommand
    {
        public EnviarEmailViewModel EnviarEmailViewModel { get; set; }

        public OpenEnviarEmailWindowCommand(EnviarEmailViewModel viewModel)
        {
            EnviarEmailViewModel = viewModel;
        }
    }
}
