using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFe.Core.NotasFiscais.ValueObjects
{
    public class Chave
    {
        public Chave(DateTime dataHoraEmissao, string numero, int serie, TipoEmissao tipoEmissao, string cnpjEmissor, Modelo modelo, CodigoUfIbge uf)
        {
            _dataHoraEmissao = dataHoraEmissao;
            _numero = numero;
            _serie = serie;
            _tipoEmissao = tipoEmissao;
            _cnpjEmissor = cnpjEmissor;
            _modelo = modelo;
            _uf = uf;

            CalcularChave();
        }

        private string _chave;
        private DateTime _dataHoraEmissao;
        private readonly int _serie;
        private readonly string _numero;
        private readonly TipoEmissao _tipoEmissao;
        private readonly string _cnpjEmissor;
        private readonly Modelo _modelo;
        private readonly CodigoUfIbge _uf;

        public int DigitoVerificador { get; private set; }
        public string Codigo { get; private set; }

        public string ChaveMasked
        {
            get
            {
                return string.Format("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10}",
                    _chave.Substring(0, 4),
                    _chave.Substring(4, 4),
                    _chave.Substring(8, 4),
                    _chave.Substring(12, 4),
                    _chave.Substring(16, 4),
                    _chave.Substring(20, 4),
                    _chave.Substring(24, 4),
                    _chave.Substring(28, 4),
                    _chave.Substring(32, 4),
                    _chave.Substring(36, 4),
                    _chave.Substring(40, 4));
            }
        }

        private string CalcularChaveSemDv()
        {
            var anoMes = _dataHoraEmissao.ToString("yyMM");
            var numNfe = PreencherNumNFeComZeros(_numero, 9);
            var serie = PreencherNumNFeComZeros(_serie.ToString(), 3);
            var random = new Random();
            Codigo = random.Next(11111121, 99999989).ToString();

            var tipoEmissao = GetTipoEmissao();
            return (int)_uf + anoMes + _cnpjEmissor + (_modelo == Modelo.Modelo55 ? 55 : 65) + serie + numNfe +
                   tipoEmissao + Codigo;
        }

        public void CalcularChave()
        {
            _chave = CalcularChaveSemDv();
            DigitoVerificador = CalcularDv(_chave);
            _chave += DigitoVerificador;
        }

        private string PreencherNumNFeComZeros(string numNFe, int qtdeDigitos)
        {
            var qtdeZeros = qtdeDigitos - numNFe.Length;
            var numeroComZeros = new StringBuilder();

            for (var i = 0; i < qtdeZeros; i++) numeroComZeros.Append("0");

            numeroComZeros.Append(numNFe);
            return numeroComZeros.ToString();
        }

        private int CalcularDv(string chave)
        {
            if (chave.Length != 43) throw new ArgumentException();

            var multiplicador = 2;
            var somaPonderacoes = 0;

            for (var i = 42; i >= 0; i--)
            {
                if (multiplicador > 9) multiplicador = 2;

                somaPonderacoes += (int)char.GetNumericValue(chave[i]) * multiplicador;
                multiplicador++;
            }

            var resto = somaPonderacoes % 11;

            if (resto == 0 || resto == 1)
                return 0;

            return 11 - resto;
        }

        private int GetTipoEmissao()
        {
            switch (_tipoEmissao)
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

        public override string ToString()
        {
            return _chave;
        }
    }
}
