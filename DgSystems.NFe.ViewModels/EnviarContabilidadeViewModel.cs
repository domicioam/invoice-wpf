using System;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using NFe.Core.Utils.Zip;

namespace NFe.WPF.ViewModel
{
   public class EnviarContabilidadeViewModel : ViewModelBase
   {
       private GeradorZip _geradorZip;
       public ICommand EnviarParaContabilidadeCmd { get; set; }

      public string Ano { get; set; }
      public string Mes { get; set; }


      public EnviarContabilidadeViewModel(GeradorZip geradorZip)
      {
         EnviarParaContabilidadeCmd = new RelayCommand<Window>(EnviarParaContabilidadeCmd_Execute, null);

         Ano = DateTime.Now.ToString("yyyy");
         Mes = DateTime.Now.ToString("MM");
         _geradorZip = geradorZip;
      }

      private async void EnviarParaContabilidadeCmd_Execute(Window window)
      {
         var periodo = DateTime.ParseExact(Ano + Mes, "yyyyMM", CultureInfo.InvariantCulture);
         string path = await _geradorZip.GerarZipEnvioContabilidadeAsync(periodo);
         if (!string.IsNullOrWhiteSpace(path))
         {
            MessageBox.Show("Arquivos gerados com sucesso!", "Sucesso!", MessageBoxButton.OK, MessageBoxImage.Information);
         }
         else
         {
            MessageBox.Show("Houve um erro ao tentar gerar os arquivos para envio. Tente novamente e, se o erro persistir, contate o suporte.", "Erro!", MessageBoxButton.OK, MessageBoxImage.Information);
         }

         window.Close();
      }
   }
}
