using NFe.Core.Cadastro.Emissor;
using NFe.Core.Entitities;
using NFe.Core.Interfaces;

namespace NFe.Core.Cadastro.Emissor
{
    public class EmissorService : IEmissorService
    {
        private readonly IEmitenteRepository _emitenteRepository;

        public EmissorService(IEmitenteRepository emitenteRepository)
        {
            _emitenteRepository = emitenteRepository;
        }

        public NotasFiscais.Emissor GetEmissor()
        {
            var emitenteDb = _emitenteRepository.GetEmitente();

            var enderecoEmitente = new Endereco(emitenteDb.Logradouro, emitenteDb.Numero, emitenteDb.Bairro,
                emitenteDb.Municipio, emitenteDb.CEP, emitenteDb.UF);

            return new NotasFiscais.Emissor(emitenteDb.RazaoSocial, emitenteDb.NomeFantasia, emitenteDb.CNPJ,
                emitenteDb.InscricaoEstadual, emitenteDb.InscricaoMunicipal, emitenteDb.CNAE,
                emitenteDb.RegimeTributario, enderecoEmitente, emitenteDb.Telefone);
        }

        public EmitenteEntity GetEmitenteEntity()
        {
            var emitenteDb = _emitenteRepository.GetEmitente();

            if (emitenteDb == null)
                return null;

            return emitenteDb;
        }

        public void Salvar(EmitenteEntity emitente)
        {
            _emitenteRepository.Salvar(emitente);
        }
    }
}