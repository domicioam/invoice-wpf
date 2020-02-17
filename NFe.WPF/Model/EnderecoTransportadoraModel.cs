using EmissorNFe.Model.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NFe.Core.Entitities;

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
            var enderecoEntity = new EnderecoTransportadoraEntity();
            enderecoEntity.Bairro = enderecoModel.Bairro;
            enderecoEntity.Logradouro = enderecoModel.Logradouro;
            enderecoEntity.Municipio = enderecoModel.Municipio;
            enderecoEntity.Numero = enderecoModel.Numero;
            enderecoEntity.UF = enderecoModel.UF;

            return enderecoEntity;
        }

        public static explicit operator EnderecoTransportadoraModel(EnderecoTransportadoraEntity endEntity)
        {
            var enderecoModel = new EnderecoTransportadoraModel();
            enderecoModel.Bairro = endEntity.Bairro;
            enderecoModel.Logradouro = endEntity.Logradouro;
            enderecoModel.Municipio = endEntity.Municipio;
            enderecoModel.Numero = endEntity.Numero;
            enderecoModel.UF = endEntity.UF;

            return enderecoModel;
        }
    }
}
