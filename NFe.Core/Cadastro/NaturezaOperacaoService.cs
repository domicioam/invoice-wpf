using System.Collections.Generic;
using NFe.Core.Entitities;
using NFe.Core.Interfaces;

namespace NFe.Core.Cadastro
{
    public class NaturezaOperacaoService : INaturezaOperacaoService
    {
        private INaturezaOperacaoRepository _naturezaOperacaoRepository;

        public NaturezaOperacaoService(INaturezaOperacaoRepository naturezaOperacaoRepository)
        {
            _naturezaOperacaoRepository = naturezaOperacaoRepository;
        }

        public string GetNaturezaOperacaoPorCfop(string cfop)
        {
            return _naturezaOperacaoRepository.GetNaturezaOperacaoPorCfop(cfop).Descricao;
        }

        public List<NaturezaOperacaoEntity> GetAll()
        {
            var naturezasDB = _naturezaOperacaoRepository.GetAll();

            var naturezas = new List<NaturezaOperacaoEntity>();

            foreach (var naturezaDB in naturezasDB)
            {
                var natureza = new NaturezaOperacaoEntity
                {
                    Descricao = naturezaDB.Descricao,
                    Id = naturezaDB.Id
                };

                naturezas.Add(natureza);
            }

            return naturezas;
        }
    }
}