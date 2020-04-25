using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EmissorNFe.Model;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using NFe.Core.Cadastro.Certificado;
using NFe.Core.Utils.Assinatura;
using NFe.WPF.ViewModel.Base;
using Utils;

namespace NFe.WPF.ViewModel
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

        private ObservableCollection<Certificate> _certificadosInstalados;
        private ICertificadoService _certificadoService;
        private ICertificateManager _certificateManager;

        public ObservableCollection<Certificate> CertificadosInstalados
        {
            get { return _certificadosInstalados; }
            set { _certificadosInstalados = value; }
        }

        public ICommand AdicionarSenhaCmd { get; set; }
        public ICommand SelecionarCertificadoCmd { get; set; }
        public ICommand SalvarCmd { get; set; }
        public ICommand LoadedCmd { get; set; }

        public CertificadoViewModel(ICertificadoService certificadoService, ICertificateManager certificateManager)
        {
            CertificadosInstalados = new ObservableCollection<Certificate>(certificateManager.GetFriendlyCertificates());
            AdicionarSenhaCmd = new RelayCommand<PasswordBox>(AdicionarSenhaCmd_Execute, null);
            SelecionarCertificadoCmd = new RelayCommand(SelecionarCertificadoCmd_Execute, null);
            SalvarCmd = new RelayCommand<Window>(SalvarCmd_Execute, null);
            LoadedCmd = new RelayCommand(LoadedCmd_Execute, null);
            _certificadoService = certificadoService;
            _certificateManager = certificateManager;
        }

        private void LoadedCmd_Execute()
        {
            Certificado = (CertificadoModel)_certificadoService.GetCertificado();

            if (Certificado == null)
            {
                Certificado = new CertificadoModel();
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
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Arquivos de certificado (*.pfx)|*.pfx";

            if (openFileDialog.ShowDialog() == true)
            {
                for (int i = 0; i < openFileDialog.FileNames.Length; i++)
                {
                    CertificadoModel newCertificado = new CertificadoModel
                    {
                        Caminho = new Uri(openFileDialog.FileNames[i]).LocalPath
                    };

                    Certificado = newCertificado;
                    IsButtonAdicionarEnabled = true;
                    TextoAdicionarCertificado = Certificado.Caminho;
                }
            }
        }

        private void AdicionarSenhaCmd_Execute(PasswordBox pwBox)
        {
            if (!string.IsNullOrWhiteSpace(pwBox.Password))
            {
                var crypto = RijndaelManagedEncryption.EncryptRijndael(pwBox.Password);
                Certificado.Senha = crypto;
                var certificado = _certificateManager.GetFriendlyCertificate(Certificado.Caminho, pwBox.Password);
                Certificado.Nome = certificado.FriendlySubjectName;
                Certificado.NumeroSerial = certificado.SerialNumber;
                IsButtonSaveEnabled = true;
            }
        }

        private void SalvarCmd_Execute(Window window)
        {
            if (!IsArquivoCertificado)
            {
                Certificado.Nome = CertificadoSelecionado.FriendlySubjectName;
                Certificado.NumeroSerial = CertificadoSelecionado.SerialNumber;
                Certificado.Caminho = null;
            }

            CertificadoEntity certificado;

            if (Certificado.Id != 0)
            {
                certificado = _certificadoService.GetCertificado();
            }
            else
            {
                certificado = new CertificadoEntity();
            }

            certificado.Nome = Certificado.Nome;
            certificado.NumeroSerial = Certificado.NumeroSerial;
            certificado.Senha = Certificado.Senha;
            certificado.Caminho = Certificado.Caminho;

            _certificadoService.Salvar(certificado);
            window.Close();
        }
    }
}
