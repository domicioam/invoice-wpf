using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NFe.Core.Domain.Services;
using NFe.Core.Domain.Services.Destinatario;
using NFe.Core.Domain.Services.Identificacao;
using NFe.Core.Domain.Services.NotaFiscal;
using NFe.Core.Model.Dest;
using NFe.Core.TO;
using NFe.CoreTest.Utils;
using NFe.Repository;
using NFe.Repository.Entities;
using NFe.Repository.Repositories;
using NFe.Core.Utils;

namespace NFe.UnitTests
{
    [TestClass]
    public class EnvioEmailTest
    {
        [TestMethod]
        public void EnvioEmailDestinatario()
        {
            var idDestinatario = DestinatarioService.Salvar(new DestinatarioEntity() { NomeRazao = "Domício de Araújo", Documento = "73598348134", TipoDestinatario = (int)TipoDestinatario.PessoaFisica });

            NotaFiscal notaFiscal;
            ConfiguracaoEntity config;
            var DestinatarioEntity = DestinatarioService.GetDestinatarioByID(idDestinatario);
            var destinatario = new Destinatario(Ambiente.Homologacao, Modelo.Modelo65, null, null, null, TipoDestinatario.PessoaFisica,
                nomeRazao: DestinatarioEntity.NomeRazao, documento: DestinatarioEntity.Documento);

            new NotaFiscalService().AtivarModoOnline();
            EnviarNotaTesteUnitarioUtils.EnviarNotaFiscal(out notaFiscal, out config, destinatario);

            var nota = new NotaFiscalService().GetNotaFiscalByChave(notaFiscal.Identificacao.Chave);

            MailManager.EnviarEmailDestinatario("domicioam@gmail.com", nota.XmlPath);

            DestinatarioService.ExcluirDestinatario(idDestinatario);
        }

        [TestMethod]
        public void EnvioEmailContabilidade()
        {
            MailManager.EnviarNotasParaContabilidade(2);
        }
    }
}
