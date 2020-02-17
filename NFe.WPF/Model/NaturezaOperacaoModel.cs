using EmissorNFe.Model.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmissorNFe.Model
{
    public class NaturezaOperacaoModel : ObservableObjectValidation
    {
        public int Id { get; set; }

        public string Descricao { get; set; }
    }
}
