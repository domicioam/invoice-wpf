using AutoMapper;
using EmissorNFe.Model;
using NFe.Core.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFe.WPF.Mapper
{
   class ConfigurationProfile : Profile
   {
      public ConfigurationProfile()
      {
         CreateMap<DestinatarioDto, DestinatarioViewModel>()
            .ReverseMap();
         CreateMap<NotaFiscalDto, NotaFiscalModel>()
            .ReverseMap();
         CreateMap<EnderecoDto, EnderecoDestinatarioModel>()
            .ReverseMap();
      }
   }
}
