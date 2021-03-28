using EmissorNFe.Model.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NFe.Core.Cadastro.Destinatario;

namespace EmissorNFe.Model
{
    public class EnderecoDestinatarioModel : ObservableObjectValidation
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
        [MinLength(2)]
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

        [RegularExpression("^$|[0-9]{8}")]
        public string CEP
        {
            get { return _cep; }
            set { SetProperty(ref _cep, value); }
        }

        public int Id { get; internal set; }

        public static explicit operator EnderecoDestinatarioModel(EnderecoDestinatarioEntity endTO)
        {
            return new EnderecoDestinatarioModel
            {
                Id = endTO.Id,
                Bairro = endTO.Bairro,
                Logradouro = endTO.Logradouro,
                Municipio = endTO.Municipio,
                Numero = endTO.Numero,
                UF = endTO.UF,
                CEP = endTO.CEP
            };
        }

        public static explicit operator EnderecoDestinatarioEntity(EnderecoDestinatarioModel endModel)
        {
            return new EnderecoDestinatarioEntity
            {
                Id = endModel.Id,
                Bairro = endModel.Bairro,
                Logradouro = endModel.Logradouro,
                Municipio = endModel.Municipio,
                Numero = endModel.Numero,
                UF = endModel.UF,
                CEP = endModel.CEP
            };
        }
    }
}
