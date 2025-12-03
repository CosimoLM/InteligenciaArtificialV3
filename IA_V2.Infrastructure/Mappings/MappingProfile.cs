using AutoMapper;
using IA_V2.Core.Entities;
using IA_V2.Infrastructure.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IA_V2.Infrastructure.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserDTO>().ReverseMap();
            CreateMap<Text, TextDTO>().ReverseMap();
            CreateMap<Prediction, TextDTO>().ReverseMap();
            CreateMap<Security, SecurityDTO>().ReverseMap();
        }
    }
}
