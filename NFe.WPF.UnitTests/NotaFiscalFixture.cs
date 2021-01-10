using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DgSystems.NFe.ViewModels;
using EmissorNFe.Model;
using NFe.Core.Cadastro.Imposto;
using NFe.Core.Entitities;
using NFe.Core.NotasFiscais;
using NFe.Core.NotasFiscais.Entities;
using NFe.WPF.Model;
using NFe.WPF.NotaFiscal.Model;
using Imposto = NFe.Core.Cadastro.Imposto.Imposto;

namespace NFe.WPF.UnitTests
{
    public class NotaFiscalFixture
    {
        public NFCeModel NFCeModel
        {
            get
            {
                NFCeModel notaFiscalModel = new NFCeModel();

                notaFiscalModel.Serie = "001";
                notaFiscalModel.Numero = "1";
                notaFiscalModel.ModeloNota = "NFC-e";

                notaFiscalModel.DataEmissao = DateTime.Now;
                notaFiscalModel.HoraEmissao = DateTime.Now;
                notaFiscalModel.DataSaida = DateTime.Now;
                notaFiscalModel.HoraSaida = DateTime.Now;
                notaFiscalModel.IndicadorPresenca = PresencaComprador.Presencial;
                notaFiscalModel.Finalidade = "Normal";
                notaFiscalModel.NaturezaOperacao = "Venda";
                notaFiscalModel.DestinatarioSelecionado = new DestinatarioModel();
                notaFiscalModel.Produtos = new ObservableCollection<ProdutoModel>()
            {
                new ProdutoModel()
                {
                    QtdeProduto = 1,
                    ValorUnitario = 65,
                    TotalLiquido = 65,
                    ProdutoSelecionado = new ProdutoEntity()
                    {
                        Id = 1,
                        ValorUnitario = 65,
                        Codigo = "0001",
                        Descricao = "Botijão P13",
                        GrupoImpostos = new GrupoImpostos()
                        {
                            Id = 1,
                            CFOP = "5656",
                            Descricao = "Gás Venda",
                            Impostos = Impostos
                        },
                        GrupoImpostosId = 1,
                        NCM = "27111910",
                        UnidadeComercial = "UN"
                    }
                }
            };
                notaFiscalModel.Pagamentos = new ObservableCollection<PagamentoModel>()
            {
                new PagamentoModel()
                {
                    FormaPagamento = "Dinheiro",
                    QtdeParcelas = 1,
                    ValorParcela = 65
                }
            };

                return notaFiscalModel;
            }
        }

        public NFeModel NFeModelWithPagamento
        {
            get
            {
                NFeModel notaFiscalModel = new NFeModel();

                notaFiscalModel.Serie = "001";
                notaFiscalModel.Numero = "1";
                notaFiscalModel.ModeloNota = "NF-e";

                notaFiscalModel.DataEmissao = DateTime.Now;
                notaFiscalModel.HoraEmissao = DateTime.Now;
                notaFiscalModel.DataSaida = DateTime.Now;
                notaFiscalModel.HoraSaida = DateTime.Now;
                notaFiscalModel.IndicadorPresenca = PresencaComprador.Presencial;
                notaFiscalModel.Finalidade = "Normal";
                notaFiscalModel.NaturezaOperacao = "Venda";
                notaFiscalModel.DestinatarioSelecionado = new DestinatarioModel()
                {
                    CNPJ = "25456879454314",
                    InscricaoEstadual = "123456789123",
                    IsNFe = true,
                    NomeRazao = "RAZAO SOCIAL LTDA",
                    TipoDestinatario = TipoDestinatario.PessoaJuridica,
                    Endereco = new EnderecoDestinatarioModel()
                    {
                        Bairro = "BRASILIA",
                        CEP = "70000000",
                        Logradouro = "Quadra 100 conjunto 10",
                        Municipio = "BRASÍLIA",
                        Numero = "10",
                        UF = "DF"
                    }
                };
                notaFiscalModel.Produtos = new ObservableCollection<ProdutoModel>()
                {
                    new ProdutoModel()
                    {
                        QtdeProduto = 1,
                        ValorUnitario = 65,
                        TotalLiquido = 65,
                        ProdutoSelecionado = new ProdutoEntity()
                        {
                            Id = 1,
                            ValorUnitario = 65,
                            Codigo = "0001",
                            Descricao = "Botijão P13",
                            GrupoImpostos = new GrupoImpostos()
                            {
                                Id = 1,
                                CFOP = "5656",
                                Descricao = "Gás Venda",
                                Impostos = Impostos
                            },
                            GrupoImpostosId = 1,
                            NCM = "27111910",
                            UnidadeComercial = "UN"
                        }
                    }
                };
                notaFiscalModel.Pagamentos = new ObservableCollection<PagamentoModel>()
                {
                    new PagamentoModel()
                    {
                        FormaPagamento = "Dinheiro",
                        QtdeParcelas = 1,
                        ValorParcela = 65
                    }
                };
                notaFiscalModel.TransportadoraSelecionada = new TransportadoraModel()
                {
                    CpfCnpj = "12345678965432",
                    NomeRazao = "TRANSPORTADORA LTDA",
                    InscricaoEstadual = "123456789123",
                    IsPessoaJuridica = true,
                    Endereco = new EnderecoTransportadoraModel()
                    {
                        Bairro = "BRASILIA",
                        CEP = "70000000",
                        Logradouro = "Quadra 100 conjunto 10",
                        Municipio = "BRASÍLIA",
                        Numero = "10",
                        UF = "DF"
                    }
                };
                notaFiscalModel.PlacaVeiculo = "KCT-6666";
                notaFiscalModel.UfVeiculo = "DF";
                return notaFiscalModel;
            }
        }

        public NFeModel NFeRemessa
        {
            get
            {
                NFeModel notaFiscalModel = new NFeModel();

                notaFiscalModel.Serie = "001";
                notaFiscalModel.Numero = "1";
                notaFiscalModel.ModeloNota = "NF-e";

                notaFiscalModel.DataEmissao = DateTime.Now;
                notaFiscalModel.HoraEmissao = DateTime.Now;
                notaFiscalModel.DataSaida = DateTime.Now;
                notaFiscalModel.HoraSaida = DateTime.Now;
                notaFiscalModel.IndicadorPresenca = PresencaComprador.Presencial;
                notaFiscalModel.Finalidade = "Normal";
                notaFiscalModel.NaturezaOperacao = "Venda";
                notaFiscalModel.DestinatarioSelecionado = new DestinatarioModel()
                {
                    CNPJ = "25456879454314",
                    InscricaoEstadual = "123456789123",
                    IsNFe = true,
                    NomeRazao = "RAZAO SOCIAL LTDA",
                    TipoDestinatario = TipoDestinatario.PessoaJuridica,
                    Endereco = new EnderecoDestinatarioModel()
                    {
                        Bairro = "BRASILIA",
                        CEP = "70000000",
                        Logradouro = "Quadra 100 conjunto 10",
                        Municipio = "BRASÍLIA",
                        Numero = "10",
                        UF = "DF"
                    }
                };
                notaFiscalModel.Produtos = new ObservableCollection<ProdutoModel>()
                {
                    new ProdutoModel()
                    {
                        QtdeProduto = 1,
                        ValorUnitario = 65,
                        TotalLiquido = 65,
                        ProdutoSelecionado = new ProdutoEntity()
                        {
                            Id = 1,
                            ValorUnitario = 65,
                            Codigo = "0001",
                            Descricao = "Botijão P13",
                            GrupoImpostos = new GrupoImpostos()
                            {
                                Id = 1,
                                CFOP = "5656",
                                Descricao = "Gás Venda",
                                Impostos = Impostos
                            },
                            GrupoImpostosId = 1,
                            NCM = "27111910",
                            UnidadeComercial = "UN"
                        }
                    }
                };

                notaFiscalModel.Pagamentos = new ObservableCollection<PagamentoModel>() { new PagamentoModel() { FormaPagamento = "Sem Pagamento" } };

                notaFiscalModel.TransportadoraSelecionada = new TransportadoraModel()
                {
                    CpfCnpj = "12345678965432",
                    NomeRazao = "TRANSPORTADORA LTDA",
                    InscricaoEstadual = "123456789123",
                    IsPessoaJuridica = true,
                    Endereco = new EnderecoTransportadoraModel()
                    {
                        Bairro = "BRASILIA",
                        CEP = "70000000",
                        Logradouro = "Quadra 100 conjunto 10",
                        Municipio = "BRASÍLIA",
                        Numero = "10",
                        UF = "DF"
                    }
                };
                notaFiscalModel.PlacaVeiculo = "KCT-6666";
                notaFiscalModel.UfVeiculo = "DF";
                return notaFiscalModel;
            }
        }

        public NFeModel NFeModelWithErrors
        {
            get
            {
                NFeModel notaFiscalModel = new NFeModel();

                notaFiscalModel.Serie = "001";
                notaFiscalModel.Numero = "1";
                notaFiscalModel.ModeloNota = "NF-e";

                notaFiscalModel.DataEmissao = DateTime.Now;
                notaFiscalModel.HoraEmissao = DateTime.Now;
                notaFiscalModel.DataSaida = DateTime.Now;
                notaFiscalModel.HoraSaida = DateTime.Now;
                notaFiscalModel.IndicadorPresenca = PresencaComprador.Presencial;
                notaFiscalModel.Finalidade = string.Empty;
                notaFiscalModel.NaturezaOperacao = string.Empty;
                notaFiscalModel.DestinatarioSelecionado = new DestinatarioModel()
                {
                    CNPJ = "25456879454314",
                    InscricaoEstadual = "123456789123",
                    IsNFe = true,
                    NomeRazao = "RAZAO SOCIAL LTDA",
                    TipoDestinatario = TipoDestinatario.PessoaJuridica,
                    Endereco = new EnderecoDestinatarioModel()
                    {
                        Bairro = "BRASILIA",
                        CEP = "70000000",
                        Logradouro = "Quadra 100 conjunto 10",
                        Municipio = "BRASÍLIA",
                        Numero = "10",
                        UF = "DF"
                    }
                };
                notaFiscalModel.Produtos = new ObservableCollection<ProdutoModel>()
                {
                    new ProdutoModel()
                    {
                        QtdeProduto = 1,
                        ValorUnitario = 65,
                        TotalLiquido = 65,
                        ProdutoSelecionado = new ProdutoEntity()
                        {
                            Id = 1,
                            ValorUnitario = 65,
                            Codigo = "0001",
                            Descricao = "Botijão P13",
                            GrupoImpostos = new GrupoImpostos()
                            {
                                Id = 1,
                                CFOP = "5656",
                                Descricao = "Gás Venda",
                                Impostos = Impostos
                            },
                            GrupoImpostosId = 1,
                            NCM = "27111910",
                            UnidadeComercial = "UN"
                        }
                    }
                };
                notaFiscalModel.Pagamentos = new ObservableCollection<PagamentoModel>()
                {
                    new PagamentoModel()
                    {
                        FormaPagamento = "Dinheiro",
                        QtdeParcelas = 1,
                        ValorParcela = 65
                    }
                };
                notaFiscalModel.TransportadoraSelecionada = new TransportadoraModel()
                {
                    CpfCnpj = "12345678965432",
                    NomeRazao = "TRANSPORTADORA LTDA",
                    InscricaoEstadual = "123456789123",
                    IsPessoaJuridica = true,
                    Endereco = new EnderecoTransportadoraModel()
                    {
                        Bairro = "BRASILIA",
                        CEP = "70000000",
                        Logradouro = "Quadra 100 conjunto 10",
                        Municipio = "BRASÍLIA",
                        Numero = "10",
                        UF = "DF"
                    }
                };
                notaFiscalModel.PlacaVeiculo = "KCT-6666";
                notaFiscalModel.UfVeiculo = "DF";
                return notaFiscalModel;
            }
        }

        public NFeModel NFeTotalInvalido
        {
            get
            {
                NFeModel notaFiscalModel = new NFeModel();

                notaFiscalModel.Serie = "001";
                notaFiscalModel.Numero = "1";
                notaFiscalModel.ModeloNota = "NF-e";

                notaFiscalModel.DataEmissao = DateTime.Now;
                notaFiscalModel.HoraEmissao = DateTime.Now;
                notaFiscalModel.DataSaida = DateTime.Now;
                notaFiscalModel.HoraSaida = DateTime.Now;
                notaFiscalModel.IndicadorPresenca = PresencaComprador.Presencial;
                notaFiscalModel.Finalidade = "Normal";
                notaFiscalModel.NaturezaOperacao = "Vend"; // provoca o erro de validação
                notaFiscalModel.DestinatarioSelecionado = new DestinatarioModel()
                {
                    CNPJ = "25456879454314",
                    InscricaoEstadual = "123456789123",
                    IsNFe = true,
                    NomeRazao = "RAZAO SOCIAL LTDA",
                    TipoDestinatario = TipoDestinatario.PessoaJuridica,
                    Endereco = new EnderecoDestinatarioModel()
                    {
                        Bairro = "BRASILIA",
                        CEP = "70000000",
                        Logradouro = "Quadra 100 conjunto 10",
                        Municipio = "BRASÍLIA",
                        Numero = "10",
                        UF = "DF"
                    }
                };
                notaFiscalModel.Produtos = new ObservableCollection<ProdutoModel>()
            {
                new ProdutoModel()
                {
                    QtdeProduto = 1,
                    ValorUnitario = 65,
                    TotalLiquido = 65,
                    ProdutoSelecionado = new ProdutoEntity()
                    {
                        Id = 1,
                        ValorUnitario = 65,
                        Codigo = "0001",
                        Descricao = "Botijão P13",
                        GrupoImpostos = new GrupoImpostos()
                        {
                            Id = 1,
                            CFOP = "5656",
                            Descricao = "Gás Venda",
                            Impostos = Impostos
                        },
                        GrupoImpostosId = 1,
                        NCM = "27111910",
                        UnidadeComercial = "UN"
                    }
                }
            };
                notaFiscalModel.Pagamentos = new ObservableCollection<PagamentoModel>()
            {
                new PagamentoModel()
                {
                    FormaPagamento = "Dinheiro",
                    QtdeParcelas = 1,
                    ValorParcela = 64 // valor de pagamento inválido
                }
            };
                notaFiscalModel.TransportadoraSelecionada = new TransportadoraModel()
                {
                    CpfCnpj = "12345678965432",
                    NomeRazao = "TRANSPORTADORA LTDA",
                    InscricaoEstadual = "123456789123",
                    IsPessoaJuridica = true,
                    Endereco = new EnderecoTransportadoraModel()
                    {
                        Bairro = "BRASILIA",
                        CEP = "70000000",
                        Logradouro = "Quadra 100 conjunto 10",
                        Municipio = "BRASÍLIA",
                        Numero = "10",
                        UF = "DF"
                    }
                };
                notaFiscalModel.PlacaVeiculo = "KCT-6666";
                notaFiscalModel.UfVeiculo = "DF";

                return notaFiscalModel;
            }
        }

        public List<Imposto> Impostos
        {
            get
            {
                var impostos = new List<Imposto>()
                {
                    new Imposto() { CST = "60", TipoImposto = TipoImposto.Icms },
                    new Imposto() { CST = "04", TipoImposto = TipoImposto.PIS }
                };

                return impostos;
            }
        }

        public ProdutoEntity ProdutoEntity
        {
            get
            {
                var produtoEntity = new ProdutoEntity()
                {
                    Id = 1,
                    ValorUnitario = 65,
                    Codigo = "0001",
                    Descricao = "Botijão P13",
                    GrupoImpostos = new GrupoImpostos()
                    {
                        Id = 1,
                        CFOP = "5656",
                        Descricao = "Gás Venda",
                        Impostos = Impostos
                    },
                    GrupoImpostosId = 1,
                    NCM = "27111910",
                    UnidadeComercial = "UN"
                };
                return produtoEntity;
            }
        }
    }
}
