using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFe.Core.NotasFiscais
{
    class BaseCalculoIcms : BaseCalculo
    {
        public BaseCalculoIcms(decimal valorProduto)
        {
            _valorProduto = valorProduto;
        }

        private decimal _valorProduto;

        public decimal Valor => _valorProduto;
    }
}
