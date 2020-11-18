using NFe.WPF.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DgSystems.NFe.ViewModels.Commands
{
    public class OpenCadastroProdutoWindowCommand
    {
        public OpenCadastroProdutoWindowCommand(ProdutoViewModel produtoViewModel)
        {
            ProdutoViewModel = produtoViewModel;
        }

        public ProdutoViewModel ProdutoViewModel { get; set; }
    }
}
