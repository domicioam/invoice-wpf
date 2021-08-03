using Akka.Actor;
using AutoFixture;
using MediatR;
using Moq;
using NFe.Core.Cadastro.Ibpt;
using NFe.Core.Interfaces;
using NFe.Core.NotasFiscais;
using NFe.Core.NotasFiscais.Sefaz.NfeConsulta2;
using NFe.Core.Sefaz;
using NFe.Core.Sefaz.Facades;
using NFe.WPF.NotaFiscal.Model;
using Xunit;

namespace DgSystems.NFe.ViewModels.IntegrationTests
{
    public class EnviarNotaAppServiceTests
    {
        [Fact]
        public void test()
        {
            Mock<IConfiguracaoRepository> configuracaoService = new Mock<IConfiguracaoRepository>();
            Mock<IProdutoRepository> produtoRepository = new Mock<IProdutoRepository>();
            Mock<SefazSettings> sefazSettings = new Mock<SefazSettings>();
            Mock<IEmiteNotaFiscalContingenciaFacade> emiteNotaFiscalContingenciaService = new Mock<IEmiteNotaFiscalContingenciaFacade>();
            Mock<INotaFiscalRepository> notaFiscalRepository = new Mock<INotaFiscalRepository>();
            Mock<IIbptManager> ibptManager = new Mock<IIbptManager>();
            Mock<IMediator> mediator = new Mock<IMediator>();
            Mock<IServiceFactory> serviceFactory = new Mock<IServiceFactory>();
            Mock<IConsultarNotaFiscalService> nfeConsulta = new Mock<IConsultarNotaFiscalService>();
            Mock<ActorSystem> actorSystem = new Mock<ActorSystem>(); // use actor ref factory

            var fixture = new Fixture();
            var notaFiscalModel = fixture.Build<NotaFiscalModel>().Create();
        }
    }
}
