using System.ComponentModel.DataAnnotations;
using EmissorNFe.Model.Base;

namespace DgSystems.NFe.ViewModels
{
    public class PagamentoVO : ObservableObjectValidation
    {
        private int _qtdeParcelas = 1;
        public int QtdeParcelas
        {
            get
            {
                return _qtdeParcelas;
            }
            set
            {
                SetProperty(ref _qtdeParcelas, value);
            }
        }

        private double _valorParcela;
        [Required]
        [Range(0.1, double.MaxValue)]
        public double ValorParcela
        {
            get
            {
                return _valorParcela;
            }
            set
            {
                SetProperty(ref _valorParcela , value);
            }
        }

        private string _formaPagamento;
        [Required]
        public string FormaPagamento
        {
            get
            {
                return _formaPagamento;
            }
            set
            {
                SetProperty(ref _formaPagamento , value);
            }
        }

        public string ValorTotal { get; set; }
    }
}
