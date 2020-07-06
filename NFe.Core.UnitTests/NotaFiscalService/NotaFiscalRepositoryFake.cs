using NFe.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NFe.Core.Entitities;
using NFe.Core.NotasFiscais;
using NFe.Core.NotasFiscais.Sefaz.NfeAutorizacao;

namespace NFe.Core.UnitTests.NotaFiscalService
{
    class NotaFiscalRepositoryFake : INotaFiscalRepository
    {
        List<NotaFiscalEntity> _notasFiscais;

        public NotaFiscalRepositoryFake()
        {
            _notasFiscais = new List<NotaFiscalEntity>();
        }

        public void ExcluirNota(string chave)
        {
            throw new NotImplementedException();
        }


        public List<NotaFiscalEntity> GetAll()
        {
            return _notasFiscais;
        }

        public NotaFiscalEntity GetNota(string numero, string serie, string modelo)
        {
            throw new NotImplementedException();
        }

        public NotaFiscalEntity GetNotaFiscalByChave(string chaveNFe)
        {
            throw new NotImplementedException();
        }

        public NotaFiscalEntity GetNotaFiscalByChave(string chave, int ambiente)
        {
            throw new NotImplementedException();
        }


        public NotaFiscalEntity GetNotaFiscalById(int idNotaFiscalDb, bool isLoadXmlData)
        {
            return _notasFiscais.FirstOrDefault(n => n.Id == idNotaFiscalDb);
        }

        public NotaFiscal GetNotaFiscalFromNfeProcXml(string xml)
        {
            throw new NotImplementedException();
        }

        public List<NotaFiscalEntity> GetNotasContingencia()
        {
            throw new NotImplementedException();
        }

        public List<NotaFiscalEntity> GetNotasFiscaisPorMesAno(DateTime periodo, bool v)
        {
            throw new NotImplementedException();
        }


        public List<NotaFiscal> GetNotasFiscaisPorPeriodo(DateTime periodoInicial, DateTime dateTime, bool v)
        {
            throw new NotImplementedException();
        }

        public Task<List<NotaFiscal>> GetNotasFiscaisPorPeriodoAsync(DateTime periodoInicial, DateTime dateTime, bool v)
        {
            throw new NotImplementedException();
        }

        public List<NotaFiscalEntity> GetNotasPendentes(bool isLoadXmlData)
        {
            throw new NotImplementedException();
        }


        public NotaFiscalEntity GetPrimeiraNotaEmitidaEmContingencia(DateTime dataHoraContingencia, DateTime now)
        {
            throw new NotImplementedException();
        }


        public int Salvar(NotaFiscalEntity notafiscal)
        {
            if (notafiscal.Id == 0)
                {
                    _notasFiscais.Add(notafiscal);
                    notafiscal.Id = _notasFiscais.Count();
                }
                else
                {
                    var notaFiscal = _notasFiscais.First(n => n.Id == notafiscal.Id);
                    _notasFiscais.Remove(notaFiscal);
                    _notasFiscais.Add(notaFiscal);
                }

                return notafiscal.Id;
        }

        public int Salvar(NotaFiscalEntity notaFiscalEntity, string p)
        {
            return Salvar(notaFiscalEntity);
        }

        public int SalvarNotaFiscalPendente(NotaFiscal notaFiscal, string v)
        {
            var NotaFiscalEntity = new NotaFiscalEntity();

            if (notaFiscal.Destinatario != null && notaFiscal.Destinatario.Endereco != null)
                NotaFiscalEntity.UfDestinatario = notaFiscal.Destinatario.Endereco.UF;
            else
                NotaFiscalEntity.UfDestinatario = notaFiscal.Emitente.Endereco.UF;

            NotaFiscalEntity.Destinatario = notaFiscal.Destinatario == null
                ? "CONSUMIDOR NÃO IDENTIFICADO"
                : notaFiscal.Destinatario.NomeRazao;
            NotaFiscalEntity.DocumentoDestinatario =
                notaFiscal.Destinatario == null ? null : notaFiscal.Destinatario.Documento;
            NotaFiscalEntity.Status = (int)notaFiscal.Identificacao.Status;
            NotaFiscalEntity.Chave = notaFiscal.Identificacao.Chave.ToString();
            NotaFiscalEntity.DataEmissao = notaFiscal.Identificacao.DataHoraEmissao;
            NotaFiscalEntity.Modelo = notaFiscal.Identificacao.Modelo == Modelo.Modelo55 ? "55" : "65";
            NotaFiscalEntity.Serie = notaFiscal.Identificacao.Serie.ToString();
            NotaFiscalEntity.TipoEmissao = notaFiscal.Identificacao.TipoEmissao.ToString();
            NotaFiscalEntity.ValorDesconto = notaFiscal.TotalNFe.IcmsTotal.ValorTotalDesconto;
            NotaFiscalEntity.ValorDespesas = notaFiscal.TotalNFe.IcmsTotal.ValorDespesasAcessorias;
            NotaFiscalEntity.ValorFrete = notaFiscal.TotalNFe.IcmsTotal.ValorTotalFrete;
            NotaFiscalEntity.ValorICMS = notaFiscal.TotalNFe.IcmsTotal.ValorTotalIcms;
            NotaFiscalEntity.ValorProdutos = notaFiscal.ValorTotalProdutos;
            NotaFiscalEntity.ValorSeguro = notaFiscal.TotalNFe.IcmsTotal.ValorTotalSeguro;
            NotaFiscalEntity.ValorTotal = notaFiscal.TotalNFe.IcmsTotal.ValorTotalNFe;
            NotaFiscalEntity.Numero = notaFiscal.Identificacao.Numero;

            return Salvar(NotaFiscalEntity);
        }

        public IEnumerable<NotaFiscalEntity> Take(int quantity, int page)
        {
            throw new NotImplementedException();
        }
    }
}
