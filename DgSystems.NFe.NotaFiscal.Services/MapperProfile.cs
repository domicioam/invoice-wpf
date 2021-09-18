using AutoMapper;
using NFe.Core.XmlSchemas.NfeConsulta2.Retorno;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Proc = NFe.Core.XmlSchemas.NfeRecepcaoEvento.Cancelamento.Retorno.Proc;

namespace DgSystems.NFe.Services
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<TEvento, Proc.TEvento>();
            CreateMap<TRetEvento, Proc.TRetEvento>();
        }
    }
}
