using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using NFe.Core.NotasFiscais.Services;
using NFe.WPF.ViewModel.Base;

namespace NFe.WPF.ViewModel
{
    public class ImportarXMLViewModel : ViewModelBaseValidation
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public ICommand ImportarXmlCmd { get; set; }

        private ImportadorXmlService _importadorXmlService;
        private IDialogService _dialogService;

        public ImportarXMLViewModel(ImportadorXmlService importadorXmlService, IDialogService dialogService)
        {
            ImportarXmlCmd = new RelayCommand<string>(ImportarXmlCmd_Execute);
            _dialogService = dialogService;
            _importadorXmlService = importadorXmlService;
        }

        private async void ImportarXmlCmd_Execute(string zipPath)
        {
            try
            {
                await _importadorXmlService.ImportarXmlAsync(zipPath);
                await _dialogService.ShowMessage("Notas exportadas com sucesso!", "Sucesso!");
            }
            catch(Exception e)
            {
                log.Error(e);
                await _dialogService.ShowError("Ocorreu um erro ao importar os arquivos xml.", "Erro", "OK", null);
            }
        }
    }
}
