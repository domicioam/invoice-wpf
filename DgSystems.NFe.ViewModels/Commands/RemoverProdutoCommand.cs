using NFe.WPF.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DgSystems.NFe.ViewModels.Commands
{
    class RemoverProdutoCommand
    {
        public ProdutoListItem Produto { get; }
        public RemoverProdutoCommand(ProdutoListItem produto)
        {
            Produto = produto;
        }
    }
}
