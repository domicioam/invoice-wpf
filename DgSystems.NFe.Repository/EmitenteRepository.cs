using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NFe.Core;
using NFe.Core.Domain;
using NFe.Core.Entitities;
using NFe.Core.Interfaces;

namespace NFe.Core.Cadastro.Emissor
{
    public class EmitenteRepository : IEmitenteRepository
    {
        private NFeContext _context;

        public EmitenteRepository()
        {
            _context = new NFeContext();
        }

        public int Salvar(EmitenteEntity emitente)
        {
            if (emitente.Id == 0)
            {
                _context.Emitente.Add(emitente);
            }

            _context.SaveChanges();
            return emitente.Id;
        }

        public void Excluir(EmitenteEntity emitente)
        {
            _context.Emitente.Remove(emitente);
            _context.SaveChanges();
        }

        public List<EmitenteEntity> GetAll()
        {
            return _context.Emitente.ToList();
        }

        public EmitenteEntity GetEmitente()
        {
            return _context.Emitente.FirstOrDefault();
        }

        public Domain.Emissor GetEmissor()
        {
            var emitenteDb = GetEmitente();

            var enderecoEmitente = new Endereco(emitenteDb.Logradouro, emitenteDb.Numero, emitenteDb.Bairro,
                emitenteDb.Municipio, emitenteDb.CEP, emitenteDb.UF);

            return new Domain.Emissor(emitenteDb.RazaoSocial, emitenteDb.NomeFantasia, emitenteDb.CNPJ,
                emitenteDb.InscricaoEstadual, emitenteDb.InscricaoMunicipal, emitenteDb.CNAE,
                emitenteDb.RegimeTributario, enderecoEmitente, emitenteDb.Telefone);
        }

        public EmitenteEntity GetEmitenteEntity()
        {
            var emitenteDb = GetEmitente();

            if (emitenteDb == null)
                return null;

            return emitenteDb;
        }
    }
}
