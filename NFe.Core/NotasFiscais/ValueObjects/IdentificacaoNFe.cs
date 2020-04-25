using System;
using System.ComponentModel;
using System.Text;
using NFe.Core.Entitities;

namespace NFe.Core.NotasFiscais
{
    public class IdentificacaoNFe
    {
        private readonly string _cnpjEmissor;

        private string _numero;

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

            Chave = CalcularChaveSemDV();
            DigitoVerificador = CalcularDV(Chave);
            Chave += DigitoVerificador;

            CodigoMunicipio = emitente.Endereco.CodigoMunicipio;
            NaturezaOperacao = naturezaOperacao;
            FinalidadeEmissao = finalidade;
            TipoOperacao = FinalidadeEmissao == FinalidadeEmissao.Devolucao ? TipoOperacao.Entrada : TipoOperacao.Saida;
            OperacaoDestino = OperacaoDestino.Interna; //não suporta interestadual ainda
            FormatoImpressao = isImpressaoBobina ? FormatoImpressao.Nfce : FormatoImpressao.Retrato;
            FinalidadeConsumidor = documentoDanfe.Contains("CPF") || Modelo == Modelo.Modelo65
                ? FinalidadeConsumidor.ConsumidorFinal
                : FinalidadeConsumidor.Normal;
            PresencaComprador = indicadorPresenca;
            ProcessoEmissao = ProcessoEmissao.AplicativoContribuinte;
            VersaoAplicativo = "0.0.0.1";
        }

        public CodigoUfIbge UF { get; set; }

        public string Numero
        {
            get { return _numero; }
            set
            {
                _numero = value;
                CalcularChave();
            }
        }

        public string Codigo { get; private set; }
        public string NaturezaOperacao { get; set; }
        public Modelo Modelo { get; }
        public int Serie { get; }
        public DateTime DataHoraEmissao { get; set; }
        public TipoOperacao TipoOperacao { get; set; }
        public OperacaoDestino OperacaoDestino { get; set; }
        public string CodigoMunicipio { get; set; }
        public FormatoImpressao FormatoImpressao { get; set; }
        public TipoEmissao TipoEmissao { get; set; }
        public Status Status { get; set; }

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
        public string Chave { get; private set; }

        public string ChaveMasked
        {
            get
            {
                return string.Format("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10}",
                    Chave.Substring(0, 4),
                    Chave.Substring(4, 4),
                    Chave.Substring(8, 4),
                    Chave.Substring(12, 4),
                    Chave.Substring(16, 4),
                    Chave.Substring(20, 4),
                    Chave.Substring(24, 4),
                    Chave.Substring(28, 4),
                    Chave.Substring(32, 4),
                    Chave.Substring(36, 4),
                    Chave.Substring(40, 4));
            }
        }

        internal int DigitoVerificador { get; set; }

        public string MensagemInteresseContribuinte
        {
            get
            {
                if (Status == Status.CONTINGENCIA) return "EMITIDA EM CONTINGÊNCIA";
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

        private string CalcularChaveSemDV()
        {
            var anoMes = DataHoraEmissao.ToString("yyMM");
            var numNfe = PreencherNumNFeComZeros(Numero, 9);
            var serie = PreencherNumNFeComZeros(Serie.ToString(), 3);
            var random = new Random();
            Codigo = random.Next(11111121, 99999989).ToString();

            var tipoEmissao = GetTipoEmissao();
            return (int) UF + anoMes + _cnpjEmissor + (Modelo == Modelo.Modelo55 ? 55 : 65) + serie + numNfe +
                   tipoEmissao + Codigo;
        }

        public void CalcularChave()
        {
            Chave = CalcularChaveSemDV();
            DigitoVerificador = CalcularDV(Chave);
            Chave += DigitoVerificador;
        }

        private int GetTipoEmissao()
        {
            switch (TipoEmissao)
            {
                case TipoEmissao.Normal:
                    return 1;

                case TipoEmissao.FsIa:
                    return 2;

                case TipoEmissao.Scan:
                    return 3;

                case TipoEmissao.Dpec:
                    return 4;

                case TipoEmissao.FsDa:
                    return 5;

                case TipoEmissao.SvcAn:
                    return 6;

                case TipoEmissao.SvcRs:
                    return 7;

                case TipoEmissao.ContigenciaNfce:
                    return 9;

                default:
                    throw new ArgumentException();
            }
        }

        private string PreencherNumNFeComZeros(string numNFe, int qtdeDigitos)
        {
            var qtdeZeros = qtdeDigitos - numNFe.Length;
            var numeroComZeros = new StringBuilder();

            for (var i = 0; i < qtdeZeros; i++) numeroComZeros.Append("0");

            numeroComZeros.Append(numNFe);
            return numeroComZeros.ToString();
        }

        private int CalcularDV(string chave)
        {
            if (chave.Length != 43) throw new ArgumentException();

            var multiplicador = 2;
            var somaPonderacoes = 0;

            for (var i = 42; i >= 0; i--)
            {
                if (multiplicador > 9) multiplicador = 2;

                somaPonderacoes += (int) char.GetNumericValue(chave[i]) * multiplicador;
                multiplicador++;
            }

            var resto = somaPonderacoes % 11;

            if (resto == 0 || resto == 1)
                return 0;

            return 11 - resto;
        }
    }

    public enum Modelo
    {
        Modelo55,
        Modelo65
    }

    public enum CodigoUfIbge //UF do destinatário ou de emissor?
    {
        DF = 53
    }

    public enum TipoOperacao
    {
        Entrada,
        Saida
    }

    public enum OperacaoDestino
    {
        Interna,
        Interestadual,
        Exterior
    }

    public enum FormatoImpressao
    {
        Nenhum,
        Retrato,
        Paisagem,
        Simplificado,
        Nfce,
        NfceEletronica
    }

    public enum TipoEmissao
    {
        Normal,
        FsIa,
        Scan,
        Dpec,
        FsDa,
        SvcAn,
        SvcRs,
        ContigenciaNfce
    }

    public enum Ambiente
    {
        Producao,
        Homologacao
    }

    public enum FinalidadeEmissao
    {
        Normal,
        Complementar,
        Ajuste,
        Devolucao
    }

    public enum FinalidadeConsumidor
    {
        Normal,
        ConsumidorFinal
    }

    public enum PresencaComprador
    {
        [Description("Não se Aplica")] NaoAplica,
        [Description("Presencial")] Presencial,

        [Description("Não Presencial, Internet")]
        NaoPresencialInternet,

        [Description("Não Presencial, Teleatendimento")]
        NaoPresencialTeleatendimento,
        [Description("Entrega a Domicílio")] NfceEntregaDomicilio,

        [Description("Não Presencial, Outros")]
        NaoPresencialOutros
    }

    public enum ProcessoEmissao
    {
        AplicativoContribuinte,
        AvulsaFisco,
        AvulsaContribuinteSiteFisco,
        ContribuinteAplicativoFisco
    }
}