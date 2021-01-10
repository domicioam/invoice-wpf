using EmissorNFe.Model.Base;
using NFe.Core.Entitities;
using System.ComponentModel.DataAnnotations;

namespace DgSystems.NFe.ViewModels
{
    public class ProdutoVO : ObservableObjectValidation
    {
        private ProdutoEntity _produtoSelecionado;

        [Required]
        public ProdutoEntity ProdutoSelecionado
        {
            get
            {
                return _produtoSelecionado;
            }
            set
            {
                if (value != null)
                {
                    _produtoSelecionado = value;
                    SetProperty(ref _produtoSelecionado, value);
                    ValorUnitario = _produtoSelecionado.ValorUnitario;
                }
            }
        }

        private int _qtdeProduto;

        [Required]
        [Range(1, int.MaxValue)]
        public int QtdeProduto
        {
            get { return _qtdeProduto; }
            set
            {
                SetProperty(ref _qtdeProduto, value);
                CalcularTotalBruto();
            }
        }

        private double _valorUnitario;

        [Required]
        public double ValorUnitario
        {
            get { return _valorUnitario; }
            set
            {
                SetProperty(ref _valorUnitario, value);
                CalcularTotalBruto();
            }
        }

        private double _descontos;

        [Required]
        public double Descontos
        {
            get { return _descontos; }
            set
            {
                SetProperty(ref _descontos, value);
                CalcularTotalLiquido();
            }
        }

        private double _frete;

        [Required]
        public double Frete
        {
            get { return _frete; }
            set
            {
                SetProperty(ref _frete, value);
                CalcularTotalLiquido();
            }
        }

        private double _outros;

        [Required]
        public double Outros
        {
            get { return _outros; }
            set
            {
                SetProperty(ref _outros, value);
                CalcularTotalLiquido();
            }
        }

        private double _seguro;

        [Required]
        public double Seguro
        {
            get { return _seguro; }
            set
            {
                SetProperty(ref _seguro, value);
                CalcularTotalLiquido();
            }
        }

        private double _totalBruto;

        [Required]
        public double TotalBruto
        {
            get { return _totalBruto; }
            set
            {
                SetProperty(ref _totalBruto, value);
                CalcularTotalLiquido();
            }
        }

        private double _totalLiquido;

        [Required]
        public double TotalLiquido
        {
            get { return _totalLiquido; }
            set
            {
                SetProperty(ref _totalLiquido, value);
            }
        }

        public string Descricao { get; set; }

        public void CalcularTotalBruto()
        {
            TotalBruto = QtdeProduto * ValorUnitario;
        }

        public void CalcularTotalLiquido()
        {
            TotalLiquido = TotalBruto - Descontos + Frete + Seguro + Outros;
        }
    }
}
