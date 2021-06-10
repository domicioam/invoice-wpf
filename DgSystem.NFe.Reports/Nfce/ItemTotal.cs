using System;

namespace DgSystem.NFe.Reports.Nfce
{
    [Serializable]
    public class ItemTotal
    {
        public ItemTotal(string descricao, object valor)
        {
            Descricao = descricao;
            Valor = valor;
        }

        public string Descricao { get; }
        public object Valor { get; }
    }
}