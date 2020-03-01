using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using EmissorNFe.Model;
using EmissorNFe.Model.Base;
using EmissorNFe.VO;
using NFe.Core.NotasFiscais;

namespace NFe.WPF.Model
{
    public abstract class NotaFiscalModel : ObservableObjectValidation
    {
        private string _dataAutorizacao;
        private DateTime _dataEmissao;
        private DateTime _dataSaida;
        private DestinatarioModel _destinatarioSelecionado;
        private string _documento;
        private string _finalidade;
        private DateTime _horaEmissao;
        private DateTime _horaSaida;
        private PresencaComprador _indicadorPresenca;
        private bool _isCpfCnpjFieldEnabled;
        private bool _isEstrangeiro;
        private bool _isImpressaoBobina = true;
        private string _modelo;
        private string _modeloNota;
        private string _naturezaOperacao;
        private string _numero;
        private ObservableCollection<PagamentoVO> _pagamentos;
        private ObservableCollection<ProdutoVO> _produtos;
        private string _serie;

        public string Destinatario { get; set; }
        public string UfDestinatario { get; set; }
        public string Valor { get; set; }

        public bool IsEstrangeiro
        {
            get { return _isEstrangeiro; }
            set { SetProperty(ref _isEstrangeiro, value); }
        }

        [Required]
        public ObservableCollection<ProdutoVO> Produtos
        {
            get { return _produtos; }
            set { SetProperty(ref _produtos, value); }
        }

        public bool IsCpfCnpjFieldEnabled
        {
            get { return _isCpfCnpjFieldEnabled; }
            set { SetProperty(ref _isCpfCnpjFieldEnabled, value); }
        }

        public DestinatarioModel DestinatarioSelecionado
        {
            get { return _destinatarioSelecionado; }
            set
            {
                SetProperty(ref _destinatarioSelecionado, value);
                IsCpfCnpjFieldEnabled = string.IsNullOrEmpty(value.NomeRazao);

                switch (value.TipoDestinatario)
                {
                    case TipoDestinatario.PessoaFisica:
                        Documento = value.CPF;
                        IsEstrangeiro = false;
                        break;
                    case TipoDestinatario.PessoaJuridica:
                        Documento = value.CNPJ;
                        IsEstrangeiro = false;
                        break;
                    case TipoDestinatario.Estrangeiro:
                        Documento = value.IdEstrangeiro;
                        IsEstrangeiro = true;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        [Required]
        public ObservableCollection<PagamentoVO> Pagamentos
        {
            get { return _pagamentos; }
            set { SetProperty(ref _pagamentos, value); }
        }

        public string Serie
        {
            get { return _serie; }
            set { SetProperty(ref _serie, value); }
        }

        public string Numero
        {
            get { return _numero; }
            set { SetProperty(ref _numero, value); }
        }

        public string Documento
        {
            get { return _documento; }
            set { SetProperty(ref _documento, value); }
        }

        [Required]
        public string NaturezaOperacao
        {
            get { return _naturezaOperacao; }
            set { SetProperty(ref _naturezaOperacao, value); }
        }

        [Required]
        public DateTime DataEmissao
        {
            get { return _dataEmissao; }
            set { SetProperty(ref _dataEmissao, value); }
        }

        [Required]
        public DateTime HoraEmissao
        {
            get { return _horaEmissao; }
            set { SetProperty(ref _horaEmissao, value); }
        }

        [Required]
        public DateTime DataSaida
        {
            get { return _dataSaida; }
            set { SetProperty(ref _dataSaida, value); }
        }

        [Required]
        public DateTime HoraSaida
        {
            get { return _horaSaida; }
            set { SetProperty(ref _horaSaida, value); }
        }

        public PresencaComprador IndicadorPresenca
        {
            get { return _indicadorPresenca; }
            set { SetProperty(ref _indicadorPresenca, value); }
        }

        public string Finalidade
        {
            get { return _finalidade; }
            set { SetProperty(ref _finalidade, value); }
        }

        public string Modelo
        {
            get
            {
                if (_modelo.Equals("55"))
                    return "NF-e";
                if (_modelo.Equals("65")) return "NFC-e";

                return null;
            }
            set { SetProperty(ref _modelo, value); }
        }

        public string ModeloNota
        {
            get { return _modeloNota; }
            set { SetProperty(ref _modeloNota, value); }
        }

        public bool IsImpressaoBobina
        {
            get { return _isImpressaoBobina; }
            set { SetProperty(ref _isImpressaoBobina, value); }
        }

        public string DataAutorizacao
        {
            get
            {
                if (string.IsNullOrEmpty(_dataAutorizacao))
                    return null;

                DateTime date;

                if (DateTime.TryParseExact(_dataAutorizacao, "yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out date)
                    || DateTime.TryParseExact(_dataAutorizacao, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out date))
                    return date.ToString("dd/MM/yyyy HH:mm:ss");

                return null;
            }
            set { SetProperty(ref _dataAutorizacao, value); }
        }
    }
}