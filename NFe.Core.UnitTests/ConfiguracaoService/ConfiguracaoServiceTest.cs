using NFe.Core.Domain;
using System;
using Xunit;
namespace NFe.Core.UnitTests.ConfiguracaoService
{
    public class ConfiguracaoServiceTest
    {
        //[Fact]
        //public void ObterProximoNumeroNotaFiscal_DeveriaIncrementarProximoNumeroNFCe()
        //{
        //    var configuracaoRepository = new ConfiguracaoRepositoryFake();
        //    var configuracaoService = new Cadastro.Configuracoes.ConfiguracaoService(configuracaoRepository);

        //    var numeroAnterior = configuracaoRepository.GetConfiguracao().ProximoNumNFCe;

        //    configuracaoService.SalvarPróximoNúmeroSérie(Modelo.Modelo65, Ambiente.Homologacao);
        //    string numeroAtual = configuracaoService.ObterProximoNumeroNotaFiscal(Modelo.Modelo65);

        //    Assert.NotEqual(numeroAnterior, numeroAtual);
        //    var numeroAnteriorIncrementado = (int.Parse(numeroAnterior) + 1).ToString();
        //    Assert.Equal(numeroAnteriorIncrementado, numeroAtual);
        //}

        //[Fact]
        //public void ObterProximoNumeroNotaFiscal_DeveriaIncrementarProximoNumeroSomenteNFCe()
        //{
        //    var configuracaoRepository = new ConfiguracaoRepositoryFake();
        //    var configuracaoService = new Cadastro.Configuracoes.ConfiguracaoService(configuracaoRepository);

        //    var numeroAnterior = configuracaoRepository.GetConfiguracao().ProximoNumNFe;

        //    configuracaoService.SalvarPróximoNúmeroSérie(Modelo.Modelo65, Ambiente.Homologacao);
        //    string numeroAtual = configuracaoService.ObterProximoNumeroNotaFiscal(Modelo.Modelo55);

        //    Assert.Equal(numeroAnterior, numeroAtual);
        //}

        //[Fact]
        //public void ObterProximoNumeroNotaFiscal_DeveriaIncrementarProximoNumeroSomenteNFCeHomologacao()
        //{
        //    var configuracaoRepository = new ConfiguracaoRepositoryFake();
        //    var configuracaoService = new Cadastro.Configuracoes.ConfiguracaoService(configuracaoRepository);

        //    var configuracao = configuracaoRepository.GetConfiguracao();
        //    configuracaoService.Salvar(configuracao);

        //    var numeroAnteriorProducao = configuracaoService.ObterProximoNumeroNotaFiscal(Modelo.Modelo65);

        //    configuracao = configuracaoRepository.GetConfiguracao();
        //    configuracaoService.Salvar(configuracao);

        //    configuracaoService.SalvarPróximoNúmeroSérie(Modelo.Modelo65, Ambiente.Homologacao);

        //    configuracao = configuracaoRepository.GetConfiguracao();
        //    configuracaoService.Salvar(configuracao);

        //    Assert.Equal(numeroAnteriorProducao, configuracaoService.ObterProximoNumeroNotaFiscal(Modelo.Modelo65));
        //}
    }
}
