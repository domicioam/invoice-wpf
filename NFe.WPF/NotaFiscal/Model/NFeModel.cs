using EmissorNFe.Model.Base;
using EmissorNFe.VO;
using GalaSoft.MvvmLight;
using NFe.WPF.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NFe.WPF.NotaFiscal.Model;

namespace EmissorNFe.Model
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
