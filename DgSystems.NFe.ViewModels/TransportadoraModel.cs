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
    public class TransportadoraModel : ObservableObjectValidation
    {
        public TransportadoraModel()
        {
            Endereco = new EnderecoTransportadoraModel();
        }

        private bool _isPessoaJuridica = true;
        private string _nomeRazao;
        private string _cpfCnpj;
        private EnderecoTransportadoraModel _endereco;
        private string _inscricaoEstadual;
        private string _telefone;

        public bool IsPessoaJuridica
        {
            get { return _isPessoaJuridica; }
            set { SetProperty(ref _isPessoaJuridica, value); }
        }

        [Required]
        public string NomeRazao
        {
            get { return _nomeRazao; }
            set { SetProperty(ref _nomeRazao, value); }
        }

        [Required]
        public string CpfCnpj
        {
            get { return _cpfCnpj; }
            set { SetProperty(ref _cpfCnpj, value); }
        }

        public string InscricaoEstadual
        {
            get { return _inscricaoEstadual; }
            set { SetProperty(ref _inscricaoEstadual, value); }
        }

        public int Id { get; set; }

        public EnderecoTransportadoraModel Endereco
        {
            get { return _endereco; }
            set { SetProperty(ref _endereco, value); }
        }

        public string Telefone
        {
            get { return _telefone; }
            set { SetProperty(ref _telefone, value); }
        }

        public override string ToString()
        {
            return NomeRazao;
        }

        public override bool HasErrors => base.HasErrors || Endereco.HasErrors;

        public override void ValidateModel()
        {
            base.ValidateModel();
            Endereco.ValidateModel();
        }

        public static explicit operator TransportadoraEntity(TransportadoraModel transportadoraModel)
        {
            return new TransportadoraEntity
            {
                CpfCnpj = transportadoraModel.CpfCnpj,
                Endereco = (EnderecoTransportadoraEntity)transportadoraModel.Endereco,
                IsPessoaJuridica = transportadoraModel.IsPessoaJuridica,
                NomeRazao = transportadoraModel.NomeRazao,
                InscricaoEstadual = transportadoraModel.InscricaoEstadual,
                Id = transportadoraModel.Id
            };
        }

        public static explicit operator TransportadoraModel(TransportadoraEntity transportadoraEntity)
        {
            return new TransportadoraModel
            {
                CpfCnpj = transportadoraEntity.CpfCnpj,
                Endereco = (EnderecoTransportadoraModel)transportadoraEntity.Endereco,
                IsPessoaJuridica = transportadoraEntity.IsPessoaJuridica,
                NomeRazao = transportadoraEntity.NomeRazao,
                InscricaoEstadual = transportadoraEntity.InscricaoEstadual,
                Id = transportadoraEntity.Id
            };
        }
    }
}
