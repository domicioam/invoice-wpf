using EmissorNFe.Model.Base;
using NFe.Core.Entitities;
using System.ComponentModel.DataAnnotations;

namespace EmissorNFe.Model
{
    public class EnderecoTransportadoraModel : ObservableObjectValidation
    {
        private string _logradouro;
        private string _bairro;
        private string _uf;
        private string _municipio;
        private string _numero;
        private string _cep;

        [Required]
        public string Logradouro
        {
            get { return _logradouro; }
            set { SetProperty(ref _logradouro, value); }
        }

        [Required]
        public string Bairro
        {
            get { return _bairro; }
            set { SetProperty(ref _bairro, value); }
        }

        [Required]
        public string UF
        {
            get { return _uf; }
            set { SetProperty(ref _uf, value); }
        }

        [Required]
        public string Municipio
        {
            get { return _municipio; }
            set { SetProperty(ref _municipio, value); }
        }

        [Required]
        public string Numero
        {
            get { return _numero; }
            set { SetProperty(ref _numero, value); }
        }

        public string CEP
        {
            get { return _cep; }
            set { SetProperty(ref _cep, value); }
        }

        public override string ToString()
        {
            return Logradouro + " " + Numero;
        }

        public static explicit operator EnderecoTransportadoraEntity(EnderecoTransportadoraModel enderecoModel)
        {
            return new EnderecoTransportadoraEntity
            {
                Bairro = enderecoModel.Bairro,
                Logradouro = enderecoModel.Logradouro,
                Municipio = enderecoModel.Municipio,
                Numero = enderecoModel.Numero,
                UF = enderecoModel.UF
            };
        }

        public static explicit operator EnderecoTransportadoraModel(EnderecoTransportadoraEntity endEntity)
        {
            return new EnderecoTransportadoraModel
            {
                Bairro = endEntity.Bairro,
                Logradouro = endEntity.Logradouro,
                Municipio = endEntity.Municipio,
                Numero = endEntity.Numero,
                UF = endEntity.UF
            };
        }
    }
}
