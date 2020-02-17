using System;
using System.Collections.Generic;

namespace NFe.Core.NotasFiscais
{
    public class NotaFiscal
    {
        public NotaFiscal(Emissor emitente, Destinatario destinatario, IdentificacaoNFe identificacao,
            Transporte transporte, TotalNFe totalNFe,
            InfoAdicional infoAdicional, List<Produto> produtos, List<Pagamento> pagamentos = null, string xml = null)
        {
            Emitente = emitente;
            Destinatario = destinatario;

            var codigoUF = (CodigoUfIbge) Enum.Parse(typeof(CodigoUfIbge), emitente.Endereco.UF);
            emitente.InscricaoMunicipal =
                codigoUF == CodigoUfIbge.DF ? emitente.InscricaoEstadual : emitente.InscricaoMunicipal;

            var documentoDanfe = Destinatario != null ? Destinatario.DocumentoDanfe : "CPF";

            Identificacao = identificacao;
            Pagamentos = pagamentos;
            Transporte = transporte;
            TotalNFe = totalNFe;
            InfoAdicional = infoAdicional;
            Produtos = produtos;
        }

        internal string ID { get; set; }
        public IdentificacaoNFe Identificacao { get; set; }
        public Emissor Emitente { get; set; }
        public Destinatario Destinatario { get; set; }
        public List<Produto> Produtos { get; set; }
        public List<Pagamento> Pagamentos { get; set; }
        public Transporte Transporte { get; set; }
        public InfoAdicional InfoAdicional { get; set; }
        public TotalNFe TotalNFe { get; set; }
        public string QrCodeUrl { get; set; }

        public string VersaoLayout { get; } = "4.00";

        public int QtdTotalProdutos
        {
            get { return Produtos != null ? Produtos.Count : 0; }
        }

        public double ValorTotalProdutos
        {
            get
            {
                double sum = 0;

                foreach (var prod in Produtos) sum += prod.ValorTotal;

                return sum;
            }
        }

        public string ProtocoloAutorizacao { get; set; }
        public string DhAutorizacao { get; set; }
        public bool IsEnviada { get; set; }
        public DateTime DataHoraAutorização { get; set; }

        internal void CalcularChave()
        {
            Identificacao.CalcularChave();
        }
    }
}