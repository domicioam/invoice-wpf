using NFe.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NFe.Core.Entitities;
using NFe.Core.NotasFiscais;

namespace NFe.WPF.ViewModel.Mementos
{
    public class NotaFiscalMemento
    {
        private NotaFiscalMemento() { }

        public NotaFiscalMemento(string número, Modelo modelo, DateTime dataEmissão, DateTime dataAutorização, string destinatário, string uf, string valor, Status status, string chave)
        {
            Número = número;
            Tipo = modelo == Modelo.Modelo55 ? "NF-e" : "NFC-e";
            DataEmissão = dataEmissão;
            DataAutorização = dataAutorização;
            Destinatário = destinatário;
            UfDestinatário = uf;
            Valor = valor;

            switch (status)
            {
                case Core.Entitities.Status.ENVIADA:
                    Status = "Enviada";
                    break;
                case Core.Entitities.Status.CONTINGENCIA:
                    Status = "Contingência";
                    break;
                case Core.Entitities.Status.PENDENTE:
                    Status = "Pendente";
                    break;
                case Core.Entitities.Status.CANCELADA:
                    Status = "Cancelada";
                    break;
            }

            Chave = chave;
        }

        public string Número { get; private set; }
        public string Tipo { get; private set; }
        public DateTime DataEmissão { get; private set; }
        public DateTime DataAutorização { get; private set; }
        public string Destinatário { get; private set; }
        public string UfDestinatário { get; private set; }
        public string Valor { get; private set; }
        public string Status { get; private set; }
        public string Chave { get; private set; }
    }
}
