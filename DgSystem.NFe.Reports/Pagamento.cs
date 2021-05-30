using System;

namespace DgSystem.NFe.Reports
{
    [Serializable]
    public class Pagamento
    {
        public Pagamento(string nome, double valor)
        {
            Nome = nome;
            Valor = valor;
        }

        public string Nome { get; }
        public double Valor { get; }
    }
}