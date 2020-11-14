using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFe.Core.NotasFiscais
{
    class BaseCalculoFundoCombatePobreza
    {
        private decimal _valorProduto;

        public BaseCalculoFundoCombatePobreza(decimal valorProduto)
        {
            _valorProduto = valorProduto;
        }


        public decimal Valor { get { return _valorProduto; } }
    }
}
