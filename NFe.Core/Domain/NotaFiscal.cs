using System;
using System.Collections.Generic;
using System.Linq;
using NFe.Core.Domain;

namespace NFe.Core.Domain
{
    public class NotaFiscal
    {
        public NotaFiscal(Emissor emitente, Destinatario destinatario, IdentificacaoNFe identificacao,
            Transporte transporte, IcmsTotal icmsTotal, IssqnTotal issqnTotal, RetencaoTributosFederais retencaoTributosFederais,
            InfoAdicional infoAdicional, List<Produto> produtos, List<Pagamento> pagamentos = null)
        {
            Emitente = emitente;
            Destinatario = destinatario;

            var codigoUF = (CodigoUfIbge) Enum.Parse(typeof(CodigoUfIbge), emitente.Endereco.UF);
            emitente.InscricaoMunicipal = codigoUF == CodigoUfIbge.DF ? emitente.InscricaoEstadual : emitente.InscricaoMunicipal;

            Identificacao = identificacao;
            Pagamentos = pagamentos;
            Transporte = transporte;
            IcmsTotal = icmsTotal;
            IssqnTotal = issqnTotal;
            RetencaoTributosFederais = retencaoTributosFederais;

            InfoAdicional = infoAdicional;
            Produtos = produtos;

            if (Identificacao.Ambiente == Ambiente.Homologacao)
                Produtos[0].Descricao = "NOTA FISCAL EMITIDA EM AMBIENTE DE HOMOLOGACAO - SEM VALOR FISCAL";
        }

        internal string ID { get; set; }
        public IdentificacaoNFe Identificacao { get; set; }
        public Emissor Emitente { get; set; }
        public Destinatario Destinatario { get; set; }
        public List<Produto> Produtos { get; set; }
        public List<Pagamento> Pagamentos { get; set; }
        public Transporte Transporte { get; set; }
        public InfoAdicional InfoAdicional { get; set; }
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
                return Produtos.Sum(p => p.ValorTotal);
            }
        }

        public string ProtocoloAutorizacao { get; set; }
        public string DhAutorizacao { get; set; }
        public bool IsEnviada { get; set; }
        public DateTime DataHoraAutorização { get; set; }
        public IcmsTotal IcmsTotal { get; set; }
        public IssqnTotal IssqnTotal { get; set; }
        public RetencaoTributosFederais RetencaoTributosFederais { get; set; }

        public void CalcularChave()
        {
            Identificacao.Chave.CalcularChave();
        }

        public double GetTotal()
        {
            return IcmsTotal.ValorTotalNFe;
        }

        public double GetTotalIcms()
        {
            return IcmsTotal.ValorTotalIcms;
        }

        public bool IsContingency()
        {
            return Identificacao.TipoEmissao == TipoEmissao.ContigenciaNfce ||
                   Identificacao.TipoEmissao == TipoEmissao.FsDa;
        }

        public bool ProdutoÉCombustível(int i)
        {
            return Identificacao.Modelo != Modelo.Modelo65 && Produtos[i].Ncm.Equals("27111910");
        }
    }
}
 