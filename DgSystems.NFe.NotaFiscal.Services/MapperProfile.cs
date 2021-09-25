using AutoMapper;
using NFe.Core.XmlSchemas.NfeConsulta2.Retorno;
using Autorizacao = NFe.Core.XmlSchemas.NfeAutorizacao.Retorno;
using Cancelamento = NFe.Core.XmlSchemas.NfeRecepcaoEvento.Cancelamento.Retorno.Proc;
using Consulta = NFe.Core.XmlSchemas.NfeConsulta2.Retorno;
using RetAutorizacao = NFe.Core.XmlSchemas.NfeRetAutorizacao.Retorno;

namespace DgSystems.NFe.Services
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<TEvento, Cancelamento.TEvento>();
            CreateMap<TRetEvento, Cancelamento.TRetEvento>();
            CreateMap<Consulta.TProtNFe, Autorizacao.TProtNFe>();
            CreateMap<RetAutorizacao.TProtNFe, Consulta.TProtNFe>();
    }
    }
}
