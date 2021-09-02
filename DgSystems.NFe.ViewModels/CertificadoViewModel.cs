using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using NFe.Core.Cadastro.Certificado;
using NFe.Core.Interfaces;
using NFe.Core.Utils;
using NFe.Core.Utils.Assinatura;
using NFe.WPF.ViewModel.Base;

namespace DgSystems.NFe.ViewModels
{
    public class CertificadoViewModel : ViewModelBaseValidation
    {
        private bool _isSenhaAdicionada;

        private string _textoAdicionarCertificado = "Clique para adicionar...";
        private bool _isArquivoCertificado;
        private bool _isCertificadoInstalado;
        private CertificadoModel _certificado;
        private bool _isButtonAdicionarEnabled;
        private Certificate _certificadoSelecionado = new Certificate();
        private bool _isButtonSaveEnabled;

        public bool IsButtonSaveEnabled
        {
            get { return _isButtonSaveEnabled; }
            set { SetProperty(ref _isButtonSaveEnabled, value); }
        }

        public string TextoAdicionarCertificado
        {
            get { return _textoAdicionarCertificado; }
            set
            {
                SetProperty(ref _textoAdicionarCertificado, value);
            }
        }

        private protected string CaminhoCertificado
        {
            get;
            set;
        }

        public bool IsButtonAdicionarEnabled
        {
            get { return _isButtonAdicionarEnabled; }
            set { SetProperty(ref _isButtonAdicionarEnabled, value); }
        }

        public Certificate CertificadoSelecionado
        {
            get { return _certificadoSelecionado; }
            set
            {
                SetProperty(ref _certificadoSelecionado, value);
                ValidarCertificadoInstalado();
            }
        }

        private void ValidarCertificadoInstalado()
        {
            IsButtonSaveEnabled = CertificadoSelecionado != null && !string.IsNullOrWhiteSpace(CertificadoSelecionado.SerialNumber);
        }

        public bool IsArquivoCertificado
        {
            get { return _isArquivoCertificado; }
            set
            {
                CertificadoSelecionado = null;
                ValidarCertificadoArquivo();
                SetProperty(ref _isArquivoCertificado, value);
            }
        }

        private void ValidarCertificadoArquivo()
        {
            IsButtonSaveEnabled = _isSenhaAdicionada;
        }

        public bool IsCertificadoInstalado
        {
            get { return _isCertificadoInstalado; }
            set
            {
                _isSenhaAdicionada = false;
                ValidarCertificadoInstalado();
                SetProperty(ref _isCertificadoInstalado, value);
            }
        }

        public CertificadoModel Certificado
        {
            get { return _certificado; }
            internal set { SetProperty(ref _certificado, value); }
        }

        private readonly ICertificadoRepository _certificadoRepository;
        private readonly CertificadoService _certificateManager;

        public ObservableCollection<Certificate> CertificadosInstalados { get; set; }

        public ICommand AdicionarSenhaCmd { get; set; }
        public ICommand SelecionarCertificadoCmd { get; set; }
        public ICommand SalvarCmd { get; set; }
        public ICommand LoadedCmd { get; set; }

        public CertificadoViewModel(ICertificadoRepository certificadoService, CertificadoService certificateManager)
        {
            CertificadosInstalados = new ObservableCollection<Certificate>(certificateManager.GetFriendlyCertificates());
            AdicionarSenhaCmd = new RelayCommand<PasswordBox>(AdicionarSenhaCmd_Execute, null);
            SelecionarCertificadoCmd = new RelayCommand(SelecionarCertificadoCmd_Execute, null);
            SalvarCmd = new RelayCommand<Window>(SalvarCmd_Execute, null);
            LoadedCmd = new RelayCommand(LoadedCmd_Execute, null);
            _certificadoRepository = certificadoService;
            _certificateManager = certificateManager;
        }

        private void LoadedCmd_Execute()
        {
            Certificado = (CertificadoModel)_certificadoRepository.GetCertificado();

            if (Certificado == null)
            {
                Certificado = CertificadoModel.CreateWithoutParameters();
            }
            else
            {
                if (!string.IsNullOrEmpty(Certificado.Caminho))
                {
                    IsArquivoCertificado = true;
                    TextoAdicionarCertificado = Certificado.Caminho;
                }
                else
                {
                    IsCertificadoInstalado = true;
                    TextoAdicionarCertificado = "Clique para adicionar...";
                    CertificadoSelecionado = CertificadosInstalados.FirstOrDefault(c => c.SerialNumber == Certificado.NumeroSerial);
                }
            }
        }

        private void SelecionarCertificadoCmd_Execute()
        {
            var openFileDialog = new OpenFileDialog { Filter = "Arquivos de certificado (*.pfx)|*.pfx" };

            if (openFileDialog.ShowDialog() != true) 
                return;
            
            foreach (var fileName in openFileDialog.FileNames)
            {
                CaminhoCertificado = new Uri(fileName).LocalPath;
                IsButtonAdicionarEnabled = true;
                TextoAdicionarCertificado = Certificado.Caminho;
            }
        }

        private void AdicionarSenhaCmd_Execute(PasswordBox pwBox)
        {
            if (string.IsNullOrWhiteSpace(pwBox.Password)) 
                return;
            
            var crypto = RijndaelManagedEncryption.EncryptRijndael(pwBox.Password);
            var certificado = _certificateManager.GetFriendlyCertificate(CaminhoCertificado, pwBox.Password);

            if (IsArquivoCertificado)
            {
                Certificado = CertificadoModel.CreateCertificadoArquivoLocal(certificado.FriendlySubjectName,
                    CaminhoCertificado, certificado.SerialNumber, crypto);
            }
            else
            {
                Certificado = CertificadoModel.CreateCertificadoInstalado(certificado.FriendlySubjectName,
                    certificado.SerialNumber, crypto);
            }

            IsButtonSaveEnabled = true;
        }

        private void SalvarCmd_Execute(Window window)
        {
            var certificado = Certificado.Id != 0 ? _certificadoRepository.GetCertificado() : new CertificadoEntity();

            certificado.Nome = Certificado.Nome;
            certificado.NumeroSerial = Certificado.NumeroSerial;
            certificado.Senha = Certificado.Senha;
            certificado.Caminho = Certificado.Caminho;

            _certificadoRepository.Salvar(certificado);
            window.Close();
        }
    }
}
