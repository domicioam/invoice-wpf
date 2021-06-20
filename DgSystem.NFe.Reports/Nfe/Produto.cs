using System;

namespace DgSystem.NFe.Reports.Nfe
{
    [Serializable]
    public class Produto
    {
        public Produto(string codigo, string descricao, string ncm, string cst, string cfop, string unidadeComercial, 
            int quantidade, double valorUnitario, double valorDesconto, double valorLiquido)
        {
            Codigo = codigo;
            Descricao = descricao;
            Ncm = ncm;
            Cst = cst;
            Cfop = cfop;
            UnidadeComercial = unidadeComercial;
            Quantidade = quantidade;
            ValorUnitario = valorUnitario;
            ValorDesconto = valorDesconto;
            ValorLiquido = valorLiquido;
        }

        public string Codigo { get;  }
        public string Descricao { get;  }
        public string Ncm { get;  }
        public string Cst { get;  }
        public string Cfop { get;  }
        public string UnidadeComercial { get;  }
        public int Quantidade { get;  }
        public double ValorUnitario { get;  }
        public double ValorDesconto { get;  }
        public double ValorLiquido { get;  }
    }
}