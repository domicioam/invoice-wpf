using EmissorNFe.Model.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NFe.Core.NotasFiscais;
using NFe.Core.Cadastro.Destinatario;
using NFe.Core.NotasFiscais.Entities;

namespace EmissorNFe.Model
{
    public class DestinatarioModel : ObservableObjectValidation
    {
        public DestinatarioModel()
        {
            Endereco = new EnderecoDestinatarioModel();
        }

        private TipoDestinatario _tipoDestinatario;

        public TipoDestinatario TipoDestinatario
        {
            get { return _tipoDestinatario; }
            set { _tipoDestinatario = value; }
        }

        private string _nomeRazao;
        private string _idEstrangeiro;
        private string _cpf;
        private string _cnpj;
        private EnderecoDestinatarioModel _endereco;
        private string _telefone;
        private string _email;
        private string _inscricaoEstadual;
        private bool _isNFe;

        [Required]
        [MinLength(2)]
        [MaxLength(60)]
        public string NomeRazao
        {
            get { return _nomeRazao; }
            set { SetProperty(ref _nomeRazao, value); }
        }

        [Required]
        [MinLength(11)]
        [MaxLength(11)]
        public string CPF
        {
            get { return _cpf; }
            set { SetProperty(ref _cpf, value); }
        }

        [Required]
        [MinLength(14)]
        [MaxLength(14)]
        public string CNPJ
        {
            get { return _cnpj; }
            set { SetProperty(ref _cnpj, value); }
        }

        [Required]
        [MinLength(5)]
        [MaxLength(20)]
        public string IdEstrangeiro
        {
            get { return _idEstrangeiro; }
            set { SetProperty(ref _idEstrangeiro, value); }
        }

        public bool IsNFe
        {
            get { return _isNFe; }
            set { SetProperty(ref _isNFe, value); }
        }

        public EnderecoDestinatarioModel Endereco
        {
            get { return _endereco; }
            set { SetProperty(ref _endereco, value); }
        }

        [RegularExpression("^$|[0-9]{6,14}")]
        public string Telefone
        {
            get { return _telefone; }
            set { SetProperty(ref _telefone, value); }
        }

        [EmailAddress]
        public string Email
        {
            get { return _email; }
            set { SetProperty(ref _email, value); }
        }

        public override string ToString()
        {
            return NomeRazao;
        }

        public override bool HasErrors => base.HasErrors || Endereco.HasErrors;

        [Required]
        public string InscricaoEstadual
        {
            get { return _inscricaoEstadual; }
            set { SetProperty(ref _inscricaoEstadual, value); }
        }

        private int _id;

        public int Id
        {
            get { return _id; }
            internal set { SetProperty(ref _id, value); }
        }

        public string Documento
        {
            get
            {
                switch (TipoDestinatario)
                {
                    case TipoDestinatario.PessoaFisica:
                        return CPF;
                    case TipoDestinatario.PessoaJuridica:
                        return CNPJ;
                    case TipoDestinatario.Estrangeiro:
                        return IdEstrangeiro;

                }
                return null;
            }
        }

        public override void ValidateModel()
        {
            base.ValidateModel();

            RemoveValidationErrors(new List<string>() { "CPF", "CNPJ", "IdEstrangeiro", "InscricaoEstadual" });

            switch (TipoDestinatario)
            {
                case TipoDestinatario.PessoaFisica:
                    ValidateProperty("CPF", CPF);
                    break;
                case TipoDestinatario.PessoaJuridica:
                    ValidateProperty("CNPJ", CNPJ);
                    if (IsNFe)
                    {
                        ValidateProperty("InscricaoEstadual", InscricaoEstadual);
                    }
                    break;
                case TipoDestinatario.Estrangeiro:
                    ValidateProperty("IdEstrangeiro", IdEstrangeiro);
                    break;
            }

            if (!string.IsNullOrWhiteSpace(Endereco.Logradouro)
                || !string.IsNullOrWhiteSpace(Endereco.Numero)
                || !string.IsNullOrWhiteSpace(Endereco.Bairro)
                || !string.IsNullOrWhiteSpace(Endereco.Municipio)
                || !string.IsNullOrWhiteSpace(Endereco.CEP)
                || IsNFe)
            {
                Endereco.ValidateModel();
            }
        }

        public static explicit operator DestinatarioModel(DestinatarioEntity destTO)
        {
            var destModel = new DestinatarioModel();

            switch (destTO.TipoDestinatario)
            {
                case (int)TipoDestinatario.PessoaFisica:
                    destModel.CPF = destTO.Documento;
                    break;
                case (int)TipoDestinatario.PessoaJuridica:
                    destModel.CNPJ = destTO.Documento;
                    break;
                case (int)TipoDestinatario.Estrangeiro:
                    destModel.IdEstrangeiro = destTO.Documento;
                    break;
            }

            destModel.Id = destTO.Id;
            destModel.Email = destTO.Email;
            destModel.Endereco = destTO.Endereco != null ? (EnderecoDestinatarioModel)destTO.Endereco : new EnderecoDestinatarioModel();
            destModel.TipoDestinatario = (TipoDestinatario)destTO.TipoDestinatario;
            destModel.NomeRazao = destTO.NomeRazao;
            destModel.Telefone = destTO.Telefone;
            destModel.InscricaoEstadual = destTO.InscricaoEstadual;

            return destModel;
        }

        public static explicit operator DestinatarioEntity(DestinatarioModel destModel)
        {
            var destTO = new DestinatarioEntity();

            switch (destModel.TipoDestinatario)
            {
                case TipoDestinatario.PessoaFisica:
                    destTO.Documento = destModel.CPF;
                    break;
                case TipoDestinatario.PessoaJuridica:
                    destTO.Documento = destModel.CNPJ;
                    break;
                case TipoDestinatario.Estrangeiro:
                    destTO.Documento = destModel.IdEstrangeiro;
                    break;
            }

            destTO.Id = destModel.Id;
            destTO.Email = destModel.Email;
            destTO.TipoDestinatario = (int)destModel.TipoDestinatario;
            destTO.NomeRazao = destModel.NomeRazao;
            destTO.Telefone = destModel.Telefone;
            destTO.InscricaoEstadual = destModel.InscricaoEstadual;

            if (!string.IsNullOrWhiteSpace(destModel.Endereco.Logradouro))
            {
                destTO.Endereco = (EnderecoDestinatarioEntity)destModel.Endereco;
            }

            return destTO;
        }
    }
}
