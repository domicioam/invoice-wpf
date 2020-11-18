using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using NFe.Core.Cadastro.Emissor;
using NFe.Core.Entitities;
using NFe.WPF.ViewModel.Base;

namespace NFe.WPF.ViewModel
{
    public class EmitenteViewModel : ViewModelBaseValidation
    {
        private string _razaoSocial;

        [Required]
        public string RazaoSocial
        {
            get { return _razaoSocial; }
            set { SetProperty(ref _razaoSocial, value); }
        }

        private string _cnpj;

        [Required]
        public string CNPJ
        {
            get { return _cnpj; }
            set { SetProperty(ref _cnpj, value); }
        }

        private string _nomeFantasia;

        [Required]
        public string NomeFantasia
        {
            get { return _nomeFantasia; }
            set { SetProperty(ref _nomeFantasia, value); }
        }

        [Required]
        private string _inscricaoEstadual;

        public string InscricaoEstadual
        {
            get { return _inscricaoEstadual; }
            set { SetProperty(ref _inscricaoEstadual, value); }
        }

        private string _regimeTributario;

        [Required]
        public string RegimeTributario
        {
            get { return _regimeTributario; }
            set { SetProperty(ref _regimeTributario, value); }
        }

        [Required]
        private string _cnae;

        public string CNAE
        {
            get { return _cnae; }
            set { SetProperty(ref _cnae, value); }
        }

        private string _cep;

        [Required]
        public string CEP
        {
            get { return _cep; }
            set { SetProperty(ref _cep, value); }
        }

        private string _logradouro;

        [Required]
        public string Logradouro
        {
            get { return _logradouro; }
            set { SetProperty(ref _logradouro, value); }
        }

        [Required]
        private string _numero;

        public string Numero
        {
            get { return _numero; }
            set { SetProperty(ref _numero, value); }
        }

        [Required]
        private string _bairro;

        public string Bairro
        {
            get { return _bairro; }
            set { SetProperty(ref _bairro, value); }
        }

        private string _uf;

        [Required]
        public string UF
        {
            get { return _uf; }
            set { SetProperty(ref _uf, value); }
        }

        private string _municipio;

        [Required]
        public string Municipio
        {
            get { return _municipio; }
            set { SetProperty(ref _municipio, value); }
        }

        private string _telefone;

        public string Telefone
        {
            get { return _telefone; }
            set { SetProperty(ref _telefone, value); }
        }

        private string _contato;
        private IEmissorService _emissorService;

        public string Contato
        {
            get { return _contato; }
            set { SetProperty(ref _contato, value); }
        }

        public List<string> RegimesTributarios { get; set; }
        public ICommand LoadedCmd { get; set; }
        public ICommand SalvarEmpresaCmd { get; set; }

        public EmitenteViewModel(IEmissorService emissorService)
        {
            SalvarEmpresaCmd = new RelayCommand<Window>(SalvarEmpresaCmd_Execute, null);
            LoadedCmd = new RelayCommand<string>(LoadedCmd_Execute, null);
            RegimesTributarios = GetRegimesTributarios();
            _emissorService = emissorService;
        }

        private List<string> GetRegimesTributarios()
        {
            return new List<string>()
                    {
                        "Simples Nacional",
                        "Simples Nacional Excesso Receita Bruta",
                        "Regime Normal"
                    };
        }

        private void SalvarEmpresaCmd_Execute(Window source)
        {
            ValidateModel();

            if (!HasErrors)
            {
                EmitenteEntity emitente = _emissorService.GetEmitenteEntity();
                emitente = emitente ?? new EmitenteEntity();

                emitente.Bairro = Bairro;
                emitente.CEP = CEP;
                emitente.CNAE = CNAE;
                emitente.CNPJ = CNPJ;
                emitente.Contato = Contato;
                emitente.Logradouro = Logradouro;
                emitente.Numero = Numero;
                emitente.InscricaoEstadual = InscricaoEstadual;
                emitente.InscricaoMunicipal = InscricaoEstadual;
                emitente.Municipio = Municipio;
                emitente.NomeFantasia = NomeFantasia;
                emitente.RazaoSocial = RazaoSocial;
                emitente.RegimeTributario = RegimeTributario;
                emitente.Telefone = Telefone;
                emitente.UF = UF;

                _emissorService.Salvar(emitente);
                source.Close();
            }
        }

        private void LoadedCmd_Execute(string obj)
        {
            var emitente = _emissorService.GetEmitenteEntity();

            if (emitente == null)
                return;

            Bairro = emitente.Bairro;
            CEP = emitente.CEP;
            CNAE = emitente.CNAE;
            CNPJ = emitente.CNPJ;
            Contato = emitente.Contato;
            Logradouro = emitente.Logradouro;
            Numero = emitente.Numero;
            InscricaoEstadual = emitente.InscricaoEstadual;
            Municipio = emitente.Municipio;
            NomeFantasia = emitente.NomeFantasia;
            RazaoSocial = emitente.RazaoSocial;
            RegimeTributario = emitente.RegimeTributario;
            Telefone = emitente.Telefone;
            UF = emitente.UF;
        }
    }
}
