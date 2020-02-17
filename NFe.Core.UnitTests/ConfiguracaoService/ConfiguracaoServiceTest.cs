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

            var numeroAnterior = configuracaoRepository.GetConfiguracao().ProximoNumNFCeHom;

            configuracaoService.SalvarPróximoNúmeroSérie(NotasFiscais.Modelo.Modelo65, NotasFiscais.Ambiente.Homologacao);
            string numeroAtual = configuracaoService.ObterProximoNumeroNotaFiscal(NotasFiscais.Modelo.Modelo65);

            Assert.NotEqual(numeroAnterior, numeroAtual);
            var numeroAnteriorIncrementado = (int.Parse(numeroAnterior) + 1).ToString();
            Assert.Equal(numeroAnteriorIncrementado, numeroAtual);
        }

        [Fact]
        public void ObterProximoNumeroNotaFiscal_DeveriaIncrementarProximoNumeroSomenteNFCe()
        {
            var configuracaoRepository = new ConfiguracaoRepositoryFake();
            var configuracaoService = new Cadastro.Configuracoes.ConfiguracaoService(configuracaoRepository);

            var numeroAnterior = configuracaoRepository.GetConfiguracao().ProximoNumNFeHom;

            configuracaoService.SalvarPróximoNúmeroSérie(NotasFiscais.Modelo.Modelo65, NotasFiscais.Ambiente.Homologacao);
            string numeroAtual = configuracaoService.ObterProximoNumeroNotaFiscal(NotasFiscais.Modelo.Modelo55);

            Assert.Equal(numeroAnterior, numeroAtual);
        }

        [Fact]
        public void ObterProximoNumeroNotaFiscal_DeveriaIncrementarProximoNumeroSomenteNFCeHomologacao()
        {
            var configuracaoRepository = new ConfiguracaoRepositoryFake();
            var configuracaoService = new Cadastro.Configuracoes.ConfiguracaoService(configuracaoRepository);

            var configuracao = configuracaoRepository.GetConfiguracao();
            configuracao.IsProducao = true;
            configuracaoService.Salvar(configuracao);

            Assert.True(configuracaoRepository.GetConfiguracao().IsProducao);

            var numeroAnteriorProducao = configuracaoService.ObterProximoNumeroNotaFiscal(NotasFiscais.Modelo.Modelo65);

            configuracao = configuracaoRepository.GetConfiguracao();
            configuracao.IsProducao = false;
            configuracaoService.Salvar(configuracao);

            Assert.False(configuracaoRepository.GetConfiguracao().IsProducao);

            configuracaoService.SalvarPróximoNúmeroSérie(NotasFiscais.Modelo.Modelo65, NotasFiscais.Ambiente.Homologacao);

            configuracao = configuracaoRepository.GetConfiguracao();
            configuracao.IsProducao = true;
            configuracaoService.Salvar(configuracao);

            Assert.True(configuracaoRepository.GetConfiguracao().IsProducao);
            Assert.Equal(numeroAnteriorProducao, configuracaoService.ObterProximoNumeroNotaFiscal(NotasFiscais.Modelo.Modelo65));
        }
    }
}
