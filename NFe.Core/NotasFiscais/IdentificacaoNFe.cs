using System;
using System.Collections.Generic;

namespace NFe.Core.Domain
{
    public class IdentificacaoNFe
    {
        private readonly string _cnpjEmissor;

        private string _numero;

        internal IdentificacaoNFe()
        {
        }

        public IdentificacaoNFe(CodigoUfIbge uf, DateTime dataHoraEmissao, string cnpjEmissor, Modelo modelo, int serie,
            string numeroNFe, TipoEmissao tipoEmissao, Ambiente ambiente, string codigoMunicipio,
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

            CodigoMunicipio = codigoMunicipio;
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

        public IdentificacaoNFe(CodigoUfIbge uf, DateTime dataHoraEmissao, string cnpjEmissor, Modelo modelo, int serie,
            string numeroNFe, TipoEmissao tipoEmissao, Ambiente ambiente, string codigoMunicipio,
            string naturezaOperacao,
            FinalidadeEmissao finalidade, FormatoImpressao formatoImpressao, PresencaComprador indicadorPresenca,
            FinalidadeConsumidor finalidadeConsumidor)
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

            CodigoMunicipio = codigoMunicipio;
            NaturezaOperacao = naturezaOperacao;
            FinalidadeEmissao = finalidade;
            TipoOperacao = TipoOperacao.Saida;
            OperacaoDestino = OperacaoDestino.Interna; //não suporta interestadual ainda
            FormatoImpressao = formatoImpressao;
            FinalidadeConsumidor = finalidadeConsumidor;
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

        public override bool Equals(object obj)
        {
            return obj is IdentificacaoNFe fe &&
                   _cnpjEmissor == fe._cnpjEmissor &&
                   _numero == fe._numero &&
                   UF == fe.UF &&
                   Numero == fe.Numero &&
                   NaturezaOperacao == fe.NaturezaOperacao &&
                   Modelo == fe.Modelo &&
                   Serie == fe.Serie &&
                   DataHoraEmissao == fe.DataHoraEmissao &&
                   TipoOperacao == fe.TipoOperacao &&
                   OperacaoDestino == fe.OperacaoDestino &&
                   CodigoMunicipio == fe.CodigoMunicipio &&
                   FormatoImpressao == fe.FormatoImpressao &&
                   TipoEmissao == fe.TipoEmissao &&
                   EqualityComparer<StatusEnvio>.Default.Equals(Status, fe.Status) &&
                   TipoOperacaooTexto == fe.TipoOperacaooTexto &&
                   Ambiente == fe.Ambiente &&
                   FinalidadeEmissao == fe.FinalidadeEmissao &&
                   FinalidadeConsumidor == fe.FinalidadeConsumidor &&
                   PresencaComprador == fe.PresencaComprador &&
                   ProcessoEmissao == fe.ProcessoEmissao &&
                   VersaoAplicativo == fe.VersaoAplicativo &&
                   DataHoraEntradaContigencia == fe.DataHoraEntradaContigencia &&
                   JustificativaContigencia == fe.JustificativaContigencia &&
                   EqualityComparer<Chave>.Default.Equals(Chave, fe.Chave) &&
                   MensagemInteresseContribuinte == fe.MensagemInteresseContribuinte &&
                   DataEmissao == fe.DataEmissao &&
                   DataSaida == fe.DataSaida &&
                   HoraSaida == fe.HoraSaida;
        }

        public override int GetHashCode()
        {
            int hashCode = -1251487820;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(_cnpjEmissor);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(_numero);
            hashCode = hashCode * -1521134295 + UF.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Numero);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(NaturezaOperacao);
            hashCode = hashCode * -1521134295 + Modelo.GetHashCode();
            hashCode = hashCode * -1521134295 + Serie.GetHashCode();
            hashCode = hashCode * -1521134295 + DataHoraEmissao.GetHashCode();
            hashCode = hashCode * -1521134295 + TipoOperacao.GetHashCode();
            hashCode = hashCode * -1521134295 + OperacaoDestino.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(CodigoMunicipio);
            hashCode = hashCode * -1521134295 + FormatoImpressao.GetHashCode();
            hashCode = hashCode * -1521134295 + TipoEmissao.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<StatusEnvio>.Default.GetHashCode(Status);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(TipoOperacaooTexto);
            hashCode = hashCode * -1521134295 + Ambiente.GetHashCode();
            hashCode = hashCode * -1521134295 + FinalidadeEmissao.GetHashCode();
            hashCode = hashCode * -1521134295 + FinalidadeConsumidor.GetHashCode();
            hashCode = hashCode * -1521134295 + PresencaComprador.GetHashCode();
            hashCode = hashCode * -1521134295 + ProcessoEmissao.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(VersaoAplicativo);
            hashCode = hashCode * -1521134295 + DataHoraEntradaContigencia.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(JustificativaContigencia);
            hashCode = hashCode * -1521134295 + EqualityComparer<Chave>.Default.GetHashCode(Chave);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(MensagemInteresseContribuinte);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(DataEmissao);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(DataSaida);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(HoraSaida);
            return hashCode;
        }
    }
}