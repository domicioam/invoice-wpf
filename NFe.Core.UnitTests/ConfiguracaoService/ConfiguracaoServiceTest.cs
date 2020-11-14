using System;
using Xunit;
namespace NFe.Core.UnitTests.ConfiguracaoService
{
    public class ConfiguracaoServiceTest
    {
        [Fact]
        public void ObterProximoNumeroNotaFiscal_DeveriaIncrementarProximoNumeroNFCe()
        {
            var configuracaoRepository = new ConfiguracaoRepositoryFake();
            var configuracaoService = new Cadastro.Configuracoes.ConfiguracaoService(configuracaoRepository);

            var numeroAnterior = configuracaoRepository.GetConfiguracao().ProximoNumNFCe;

            configuracaoService.SalvarPróximoNúmeroSérie(Core.NotasFiscais.Modelo.Modelo65, Core.NotasFiscais.Ambiente.Homologacao);
            string numeroAtual = configuracaoService.ObterProximoNumeroNotaFiscal(Core.NotasFiscais.Modelo.Modelo65);

            Assert.NotEqual(numeroAnterior, numeroAtual);
            var numeroAnteriorIncrementado = (int.Parse(numeroAnterior) + 1).ToString();
            Assert.Equal(numeroAnteriorIncrementado, numeroAtual);
        }

        [Fact]
        public void ObterProximoNumeroNotaFiscal_DeveriaIncrementarProximoNumeroSomenteNFCe()
        {
            var configuracaoRepository = new ConfiguracaoRepositoryFake();
            var configuracaoService = new Cadastro.Configuracoes.ConfiguracaoService(configuracaoRepository);

            var numeroAnterior = configuracaoRepository.GetConfiguracao().ProximoNumNFe;

            configuracaoService.SalvarPróximoNúmeroSérie(Core.NotasFiscais.Modelo.Modelo65, Core.NotasFiscais.Ambiente.Homologacao);
            string numeroAtual = configuracaoService.ObterProximoNumeroNotaFiscal(Core.NotasFiscais.Modelo.Modelo55);

            Assert.Equal(numeroAnterior, numeroAtual);
        }

        [Fact]
        public void ObterProximoNumeroNotaFiscal_DeveriaIncrementarProximoNumeroSomenteNFCeHomologacao()
        {
            var configuracaoRepository = new ConfiguracaoRepositoryFake();
            var configuracaoService = new Cadastro.Configuracoes.ConfiguracaoService(configuracaoRepository);

            var configuracao = configuracaoRepository.GetConfiguracao();
            configuracaoService.Salvar(configuracao);

            var numeroAnteriorProducao = configuracaoService.ObterProximoNumeroNotaFiscal(Core.NotasFiscais.Modelo.Modelo65);

            configuracao = configuracaoRepository.GetConfiguracao();
            configuracaoService.Salvar(configuracao);

            configuracaoService.SalvarPróximoNúmeroSérie(Core.NotasFiscais.Modelo.Modelo65, Core.NotasFiscais.Ambiente.Homologacao);

            configuracao = configuracaoRepository.GetConfiguracao();
            configuracaoService.Salvar(configuracao);

            Assert.Equal(numeroAnteriorProducao, configuracaoService.ObterProximoNumeroNotaFiscal(Core.NotasFiscais.Modelo.Modelo65));
        }
    }
}
