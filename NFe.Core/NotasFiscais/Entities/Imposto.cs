using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NFe.Core.Cadastro.Imposto;

namespace NFe.Core.NotasFiscais.Entities
{
    public class Imposto
    {
        public int Id { get; set; }
        public int GrupoImpostosId { get; set; }
        public double BaseCalculo { get; set; }
        public double Aliquota { get; set; }
        public double Reducao { get; set; }
        public string CST { get; set; }
        public TipoImposto? TipoImposto { get; set; }
        public Origem? Origem { get; set; }
        public decimal BaseCalculoFCP { get; set; }
        public decimal AliquotaFCP { get; set; }
        public decimal ValorDesonerado { get; set; }
        public MotivoDesoneracao MotivoDesoneracao { get; set; }
    }
}
