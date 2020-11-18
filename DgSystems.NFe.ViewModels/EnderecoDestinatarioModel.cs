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
            var enderecoModel = new EnderecoDestinatarioModel();
            enderecoModel.Id = endTO.Id;
            enderecoModel.Bairro = endTO.Bairro;
            enderecoModel.Logradouro = endTO.Logradouro;
            enderecoModel.Municipio = endTO.Municipio;
            enderecoModel.Numero = endTO.Numero;
            enderecoModel.UF = endTO.UF;

            return enderecoModel;
        }

        public static explicit operator EnderecoDestinatarioEntity(EnderecoDestinatarioModel endModel)
        {
            var endTO = new EnderecoDestinatarioEntity();
            endTO.Id = endModel.Id;
            endTO.Bairro = endModel.Bairro;
            endTO.Logradouro = endModel.Logradouro;
            endTO.Municipio = endModel.Municipio;
            endTO.Numero = endModel.Numero;
            endTO.UF = endModel.UF;

            return endTO;
        }
    }
}
