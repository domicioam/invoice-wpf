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
            var transportadoraEntity = new TransportadoraEntity();
            transportadoraEntity.CpfCnpj = transportadoraModel.CpfCnpj;
            transportadoraEntity.Endereco = (EnderecoTransportadoraEntity)transportadoraModel.Endereco;
            transportadoraEntity.IsPessoaJuridica = transportadoraModel.IsPessoaJuridica;
            transportadoraEntity.NomeRazao = transportadoraModel.NomeRazao;
            transportadoraEntity.InscricaoEstadual = transportadoraModel.InscricaoEstadual;
            transportadoraEntity.Id = transportadoraModel.Id;

            return transportadoraEntity;
        }

        public static explicit operator TransportadoraModel(TransportadoraEntity transportadoraEntity)
        {
            var transportadoraModel = new TransportadoraModel();
            transportadoraModel.CpfCnpj = transportadoraEntity.CpfCnpj;
            transportadoraModel.Endereco = (EnderecoTransportadoraModel)transportadoraEntity.Endereco;
            transportadoraModel.IsPessoaJuridica = transportadoraEntity.IsPessoaJuridica;
            transportadoraModel.NomeRazao = transportadoraEntity.NomeRazao;
            transportadoraModel.InscricaoEstadual = transportadoraEntity.InscricaoEstadual;
            transportadoraModel.Id = transportadoraEntity.Id;

            return transportadoraModel;
        }
    }
}
