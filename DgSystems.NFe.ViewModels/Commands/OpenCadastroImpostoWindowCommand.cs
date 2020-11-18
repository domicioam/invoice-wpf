using NFe.WPF.ViewModel;

namespace DgSystems.NFe.ViewModels.Commands
{
    public class OpenCadastroImpostoWindowCommand
    {
        public ImpostoViewModel ImpostoViewModel { get; set; }

        public OpenCadastroImpostoWindowCommand(ImpostoViewModel impostoViewModel)
        {
            this.ImpostoViewModel = impostoViewModel;
        }
    }
}