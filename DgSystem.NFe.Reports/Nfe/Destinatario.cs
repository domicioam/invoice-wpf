using System;

namespace DgSystem.NFe.Reports.Nfe
{
    [Serializable]
    public class Destinatario
    {
        public Destinatario(string nome, string documento, string inscricaoEstadual, string logradouro, string numero, 
            string bairro, string municipio, string uF, string cEP, string telefone)
        {
            Nome = nome;
            Documento = documento;
            InscricaoEstadual = inscricaoEstadual;
            Logradouro = logradouro;
            Numero = numero;
            Bairro = bairro;
            Municipio = municipio;
            UF = uF;
            CEP = cEP;
            Telefone = telefone;
        }

        public string Nome { get;  }
        public string Documento { get;  }
        public string InscricaoEstadual { get;  }
        public string Logradouro { get;  }
        public string Numero { get;  }
        public string Bairro { get;  }
        public string Municipio { get;  }
        public string UF { get;  }
        public string CEP { get;  }
        public string Telefone { get;  }
    }
}