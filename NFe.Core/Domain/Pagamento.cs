using NFe.Core.Domain;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace NFe.Core.Domain
{
    public enum FormaPagamento
    {
        Dinheiro,
        Cheque,
        CartaoCredito,
        CartaoDebito,

        SemPagamento = 11
        //CreditoLoja,
        //ValeAlimentacao,
        //ValeRefeicao,
        //ValePresente,
        //ValeCombustivel,
        //Outros
    }

    public class Pagamento
    {
        private FormaPagamento formaPagamento;

        public Pagamento(FormaPagamento formaPagamento, double valor)
        {
            FormaPagamento = formaPagamento;
            Valor = valor;
        }

        public Pagamento(FormaPagamento formaPagamento)
        {
            FormaPagamento = formaPagamento;
        }

        public FormaPagamento FormaPagamento
        {
            get { return formaPagamento; }
            set
            {
                formaPagamento = value;

                switch (value)
                {
                    case FormaPagamento.Dinheiro:
                        FormaPagamentoTexto = "Dinheiro";
                        break;
                    case FormaPagamento.Cheque:
                        FormaPagamentoTexto = "Cheque";
                        break;
                    case FormaPagamento.CartaoCredito:
                        FormaPagamentoTexto = "Cartão de Crédito";
                        break;
                    case FormaPagamento.CartaoDebito:
                        FormaPagamentoTexto = "Cartão de Débito";
                        break;
                    case FormaPagamento.SemPagamento:
                        FormaPagamentoTexto = "Sem Pagamento";
                        break;
                    default:
                        throw new ArgumentException();
                }
            }
        }

        public double Valor { get; set; }
        public List<Cartao> Cartoes { get; set; }

        public string ValorStr
        {
            get { return Valor.ToString("N2", CultureInfo.InvariantCulture); }
        }

        public string FormaPagamentoTexto { get; set; }
    }
}