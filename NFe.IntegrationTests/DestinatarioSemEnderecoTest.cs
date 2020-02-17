using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NFe.Core.Domain.Services.Configuracao;
using NFe.Core.Domain.Services.Identificacao;
using NFe.Core.Domain.Services.Emissor;
using NFe.Core.Model.Dest;
using NFe.Core.TO;
using NFe.Core.Domain.Services.Produto;
using System.Collections.Generic;
using NFe.Core.Domain.Services.ICMS;
using NFe.Core.Domain.Services.Pagto;
using NFe.Core.Domain.Services;
using NFe.Core.Domain.Services.Transp;
using NFe.Core.Domain.Services.NotaFiscal;
using NFe.Core;
using NFe.Core.Servicos;
using System.Globalization;
using System.IO;
using NFe.Core.Utils.Xml;
using NFe.Core.Domain.Services.Destinatario;
using NFe.CoreTest.Utils;
using NFe.Repository;
using NFe.Repository.Entities;

namespace NFe.CoreTest
{
   [TestClass]
   public class DestinatarioSemEnderecoTest
   {
      [TestMethod]
      public void EnviarNotaDestinatarioSemEndTest()
      {
         var idDestinatario = DestinatarioService.Salvar(new DestinatarioEntity() { NomeRazao = "Domício de Araújo", Documento = "73598348134", TipoDestinatario = (int)TipoDestinatario.PessoaFisica });
         Assert.AreNotEqual(0, idDestinatario);

         try
         {
            NotaFiscal notaFiscal;
            ConfiguracaoEntity config;
            var DestinatarioEntity = DestinatarioService.GetDestinatarioByID(idDestinatario);
            var destinatario = new Destinatario(Ambiente.Homologacao, Modelo.Modelo65, null, null, null, TipoDestinatario.PessoaFisica,
                nomeRazao: DestinatarioEntity.NomeRazao, documento: DestinatarioEntity.Documento);

            new NotaFiscalService().AtivarModoOnline();
            EnviarNotaTesteUnitarioUtils.EnviarNotaFiscal(out notaFiscal, out config, destinatario);

            var notaTest = new NotaFiscalService().GetNotaFiscalByChave(notaFiscal.Identificacao.Chave, Ambiente.Homologacao);

            if (ConsultaStatusServicoService.ExecutarConsultaStatus(config, Modelo.Modelo65))
            {
               Assert.IsTrue(notaTest.Status == (int)NFe.Repository.Status.ENVIADA);
            }
            else
            {
               Assert.IsTrue(notaTest.Status == (int)NFe.Repository.Status.CONTINGENCIA);
            }
         }
         finally
         {
            DestinatarioService.ExcluirDestinatario(idDestinatario);
         }
      }

      public DestinatarioSemEnderecoTest()
      {
         string appDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Notas Fiscais");

         if (!Directory.Exists(appDataDir))
         {
            Directory.CreateDirectory(appDataDir);
         }

         AppDomain.CurrentDomain.SetData("DataDirectory", appDataDir);
      }

   }
}
