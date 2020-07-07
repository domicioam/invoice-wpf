using System;
using System.Globalization;
using NFe.Core.Entitities;
using NFe.WPF.Model;

namespace NFe.WPF.NotaFiscal.Model
{
    public class NFCeModel : NotaFiscalModel
    {
        private string _chave;
        private string _protocolo;

        public string Chave
        {
            get { return _chave; }
            set { SetProperty(ref _chave, value); }
        }

        public string Protocolo
        {
            get { return _protocolo; }
            set { SetProperty(ref _protocolo, value); }
        }

        public string Status { get; set; }
        public bool IsCancelada { get; internal set; }

        public static explicit operator NFCeModel(NFe.Core.NotasFiscais.NotaFiscal nota)
        {
            var notaModel = new NFCeModel
            {
                DataAutorizacao = nota.DhAutorizacao,
                DataEmissao = nota.Identificacao.DataHoraEmissao,
                Modelo = nota.Identificacao.Modelo.ToString().Replace("Modelo", string.Empty),
                Numero = nota.Identificacao.Numero,
                Serie = nota.Identificacao.Serie.ToString(),
                Valor = nota.TotalNFe.IcmsTotal.ValorTotalNFe.ToString("N2", new CultureInfo("pt-BR")),
                Chave = nota.Identificacao.Chave.ToString(),
                Protocolo = nota.ProtocoloAutorizacao,
                IsCancelada = nota.Identificacao.Status.IsCancelada(),
                Destinatario = nota.Destinatario == null
                    ? "CONSUMIDOR NÃO IDENTIFICADO"
                    : nota.Destinatario.NomeRazao
            };


            if (nota.Destinatario != null && nota.Destinatario.Endereco != null)
            {
                notaModel.UfDestinatario = nota.Destinatario.Endereco.UF;
            }
            else
            {
                notaModel.UfDestinatario = nota.Emitente.Endereco.UF;
            }

            notaModel.Status = nota.Identificacao.Status.ToString();

            return notaModel;
        }

        public static explicit operator NFCeModel(NotaFiscalEntity nota)
        {
            //criar um conversor pra casting
            var notaModel = new NFCeModel()
            {
                DataAutorizacao = nota.DataAutorizacao.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                DataEmissao = nota.DataEmissao,
                Destinatario = nota.Destinatario,
                Modelo = nota.Modelo,
                Numero = nota.Numero,
                Serie = nota.Serie,
                UfDestinatario = nota.UfDestinatario,
                Valor = nota.ValorTotal.ToString("N2", new CultureInfo("pt-BR")),
                Chave = nota.Chave,
                Protocolo = nota.Protocolo,
                IsCancelada = nota.Status == 2
            };

            switch (nota.Status)
            {
                case 0:
                    notaModel.Status = "Enviada";
                    break;
                case 3:
                    notaModel.Status = "Contingência";
                    break;
                case 1:
                    notaModel.Status = "Pendente";
                    break;
                case 2:
                    notaModel.Status = "Cancelada";
                    break;
            }

            return notaModel;
        }
    }
}
