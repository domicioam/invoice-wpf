using System;
using System.Text;
using NFe.Core.Entitities;
using NFe.Core.NotasFiscais.ValueObjects;

namespace NFe.Core.NotasFiscais
{
    public class IdentificacaoNFe
    {
        private readonly string _cnpjEmissor;

        private string _numero;

        internal IdentificacaoNFe()
        {
        }

        public IdentificacaoNFe(CodigoUfIbge uf, DateTime dataHoraEmissao, string cnpjEmissor, Modelo modelo, int serie,
            string numeroNFe, TipoEmissao tipoEmissao, Ambiente ambiente, Emissor emitente,
            string naturezaOperacao,
            FinalidadeEmissao finalidade, bool isImpressaoBobina, PresencaComprador indicadorPresenca,
            string documentoDanfe)
        {
            _cnpjEmissor = cnpjEmissor;
            UF = uf;
            DataHoraEmissao = dataHoraEmissao;
            Modelo = modelo;
            Serie = serie;
            Numero = numeroNFe;
            TipoEmissao = tipoEmissao;
            Ambiente = ambiente;

            Chave = new Chave(DataHoraEmissao, Numero, Serie, TipoEmissao, _cnpjEmissor, Modelo, UF);

            CodigoMunicipio = emitente.Endereco.CodigoMunicipio;
            NaturezaOperacao = naturezaOperacao;
            FinalidadeEmissao = finalidade;
            TipoOperacao = TipoOperacao.Saida;
            OperacaoDestino = OperacaoDestino.Interna; //não suporta interestadual ainda
            FormatoImpressao = isImpressaoBobina ? FormatoImpressao.Nfce : FormatoImpressao.Retrato;
            FinalidadeConsumidor = documentoDanfe.Contains("CPF") || Modelo == Modelo.Modelo65
                ? FinalidadeConsumidor.ConsumidorFinal
                : FinalidadeConsumidor.Normal;
            PresencaComprador = indicadorPresenca;
            ProcessoEmissao = ProcessoEmissao.AplicativoContribuinte;
            VersaoAplicativo = "0.0.0.1";

            Status = new StatusEnvio(Entitities.Status.ENVIADA);
        }

        public CodigoUfIbge UF { get; set; }

        public string Numero
        {
            get { return _numero; }
            set
            {
                _numero = value;
                Chave = new Chave(DataHoraEmissao, Numero, Serie, TipoEmissao, _cnpjEmissor, Modelo, UF);
            }
        }

        public string NaturezaOperacao { get; set; }
        public Modelo Modelo { get; }
        public int Serie { get; }
        public DateTime DataHoraEmissao { get; set; }
        public TipoOperacao TipoOperacao { get; set; }
        public OperacaoDestino OperacaoDestino { get; set; }
        public string CodigoMunicipio { get; set; }
        public FormatoImpressao FormatoImpressao { get; set; }
        public TipoEmissao TipoEmissao { get; set; }
        public StatusEnvio Status { get; set; }

        public string TipoOperacaooTexto
        {
            get { return TipoOperacao == TipoOperacao.Entrada ? "0" : "1"; }
        }

        public Ambiente Ambiente { get; set; }
        public FinalidadeEmissao FinalidadeEmissao { get; set; }
        public FinalidadeConsumidor FinalidadeConsumidor { get; set; }
        public PresencaComprador PresencaComprador { get; set; }
        public ProcessoEmissao ProcessoEmissao { get; set; }
        public string VersaoAplicativo { get; set; }
        public DateTime DataHoraEntradaContigencia { get; set; }
        public string JustificativaContigencia { get; set; }
        public Chave Chave { get; private set; }

        public string MensagemInteresseContribuinte
        {
            get
            {
                if (Status.IsContingencia()) return "EMITIDA EM CONTINGÊNCIA";
                if (Ambiente == Ambiente.Homologacao)
                    return "EMITIDA EM AMBIENTE DE HOMOLOGAÇÃO - SEM VALOR FISCAL";
                return null;
            }
        }

        public string DataEmissao
        {
            get { return DataHoraEmissao.ToString("dd/MM/yy"); }
        }

        public string DataSaida
        {
            get { return DataHoraEmissao.ToString("dd/MM/yy"); }
        }

        public string HoraSaida
        {
            get { return DataHoraEmissao.ToShortTimeString(); }
        }

        public string LinkConsultaChave { get; set; } = @"http://dec.fazenda.df.gov.br/ConsultarNFCe.aspx";

        public byte[] QrCodeImage { get; set; }
    }
}