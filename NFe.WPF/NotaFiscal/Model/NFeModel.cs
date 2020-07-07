using System.ComponentModel.DataAnnotations;
using EmissorNFe.Model;

namespace NFe.WPF.NotaFiscal.Model
{
    public class NFeModel : NotaFiscalModel
    {
        private TransportadoraModel _transportadoraSelecionada;
        private string _placaVeiculo;
        private string _ufVeiculo;

        [Required]
        public TransportadoraModel TransportadoraSelecionada
        {
            get { return _transportadoraSelecionada; }
            set
            {
                SetProperty(ref _transportadoraSelecionada, value);
            }
        }

        [Required]
        public string PlacaVeiculo
        {
            get { return _placaVeiculo; }
            set { SetProperty(ref _placaVeiculo, value); }
        }

        [Required]
        public string UfVeiculo
        {
            get { return _ufVeiculo; }
            set { SetProperty(ref _ufVeiculo, value); }
        }
    }
}
