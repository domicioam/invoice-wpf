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
using NFe.Core.Utils.Conversores;
using NFe.CoreTest.Utils;
using NFe.Repository;
using System.Threading.Tasks;

namespace NFe.CoreTest
{
    [TestClass]
    public class XmlFileTest
    {
        [TestMethod]
        public void NotaEnviadaXmlExisteTest()
        {
            NotaFiscal notaFiscal;
            ConfiguracaoEntity config;
            EnviarNotaTesteUnitarioUtils.EnviarNotaFiscal(out notaFiscal, out config);

            //verificar se arquivo existe
            var notaTest = new NotaFiscalService().GetNotaFiscalByChave(notaFiscal.Identificacao.Chave, Ambiente.Homologacao);

            Assert.IsFalse(string.IsNullOrWhiteSpace(notaTest.LoadXml()));
            Assert.IsTrue(File.Exists(notaTest.XmlPath));

            if (ConsultaStatusServicoService.ExecutarConsultaStatus(config, Modelo.Modelo65))
            {
                Assert.IsTrue(notaTest.Status == (int)NFe.Repository.Status.ENVIADA);
            }
            else
            {
                Assert.IsTrue(notaTest.Status == (int)NFe.Repository.Status.CONTINGENCIA);
            }
        }


        [TestMethod]
        public void NotaNormalEnviadaXmlValidoTest()
        {
            NotaFiscal notaFiscal;
            ConfiguracaoEntity config;
            EnviarNotaTesteUnitarioUtils.EnviarNotaFiscal(out notaFiscal, out config);

            //verificar se arquivo existe
            var notaTest = new NotaFiscalService().GetNotaFiscalByChave(notaFiscal.Identificacao.Chave, Ambiente.Homologacao);
            string xml = notaTest.LoadXml();
            Assert.IsFalse(string.IsNullOrWhiteSpace(xml));
            Assert.IsTrue(File.Exists(notaTest.XmlPath));
            Assert.IsTrue(notaTest.Status == (int)NFe.Repository.Status.ENVIADA && ConsultaStatusServicoService.ExecutarConsultaStatus(config, Modelo.Modelo65));
            ValidadorXml.ValidarXml(xml, "procNFe_v4.00.xsd");

        }

        [TestMethod]
        public void NotaContingenciaEnviadaXmlValidoTest()
        {
            var notaFiscalService = new NotaFiscalService();

            notaFiscalService.AtivarModoOffline("Teste unitário envio contingência", DateTime.Now);
            NotaFiscal notaFiscal;
            ConfiguracaoEntity config;
            EnviarNotaTesteUnitarioUtils.EnviarNotaFiscal(out notaFiscal, out config);

            Task task = new Task(() => notaFiscalService.AtivarModoOnline());
            task.RunSynchronously();
            task.Wait();

            //verificar se arquivo existe
            var notaTest = new NotaFiscalService().GetNotaFiscalByChave(notaFiscal.Identificacao.Chave, Ambiente.Homologacao);
            string xml = notaTest.LoadXml();

            Assert.IsFalse(string.IsNullOrWhiteSpace(xml));
            Assert.IsTrue(File.Exists(notaTest.XmlPath));

            Assert.IsTrue(notaTest.Status == (int)NFe.Repository.Status.ENVIADA);
            ValidadorXml.ValidarXml(xml, "procNFe_v4.00.xsd");
        }

        [TestMethod]
        public async Task NotaCanceladaXmlExisteTest()
        {
            NotaFiscal notaFiscal;
            ConfiguracaoEntity config;
            int notaFiscalId = EnviarNotaTesteUnitarioUtils.EnviarNotaFiscal(out notaFiscal, out config);

            var notaFiscalDb = new NotaFiscalService().GetNotaFiscalByIdAsync(notaFiscalId, false).Result;

            var emitente = notaFiscal.Emitente;
            var codigoUFEnum = (CodigoUfIbge)Enum.Parse(typeof(CodigoUfIbge), emitente.Endereco.UF);

            await new NotaFiscalService().CancelarNotaFiscalAsync(emitente.Endereco.UF, codigoUFEnum, Ambiente.Homologacao, emitente.CNPJ, notaFiscal.Identificacao.Chave,
                notaFiscalDb.Protocolo, notaFiscal.Identificacao.Modelo, "Teste unitário cancelamento");

            var notaTest = new NotaFiscalService().GetNotaFiscalByChave(notaFiscal.Identificacao.Chave, Ambiente.Homologacao);
            var evento = new EventoService().GetEventoPorNota(notaTest.Id, true);

            ValidadorXml.ValidarXml(evento.LoadXml(), "procEventoCancNFe_v1.00.xsd");
            ValidadorXml.ValidarXml(notaTest.LoadXml(), "procNFe_v4.00.xsd");

            Assert.IsTrue(notaTest.Status == (int)NFe.Repository.Status.CANCELADA);
            Assert.IsTrue(File.Exists(evento.XmlPath));
            Assert.IsTrue(File.Exists(notaTest.XmlPath));
        }

        [TestMethod]
        public void NotaInutilizadaXmlExisteTest()
        {
            var config = ConfiguracaoService.GetConfiguracao();
            var emitente = EmissorService.GetEmissor();
            var codigoUFEnum = (CodigoUfIbge)Enum.Parse(typeof(CodigoUfIbge), emitente.Endereco.UF);
            var numInutilizar = config.ProximoNumNFCeHom;
            var serieInutilizar = Convert.ToInt32(config.SerieNFCeHom);

            var mensagemRetorno = new NotaInutilizadaService().InutilizarNotaFiscal(emitente.Endereco.UF, codigoUFEnum, Ambiente.Homologacao, emitente.CNPJ, Modelo.Modelo65, serieInutilizar.ToString(),
                numInutilizar, numInutilizar);

            if (mensagemRetorno.Status == NFe.Core.Servicos.Status.SUCESSO)
            {
                config.ProximoNumNFCeHom = (Convert.ToInt32(config.ProximoNumNFCeHom) + 1).ToString();
                ConfiguracaoService.Salvar(config);
            }

            var notaInutilizadaTest = new NotaInutilizadaService().GetNotaInutilizada(mensagemRetorno.IdInutilizacao, true, false);
            ValidadorXml.ValidarXml(notaInutilizadaTest.LoadXml(), "procInutNFe_v4.00.xsd");

            Assert.IsTrue(File.Exists(notaInutilizadaTest.XmlPath));
        }

        public XmlFileTest()
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