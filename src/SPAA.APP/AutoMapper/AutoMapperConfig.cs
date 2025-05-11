using AutoMapper;
using SPAA.App.ViewModels;
using SPAA.Business.Models;

namespace SPAA.App.AutoMapper
{
    public class AutoMapperConfig : Profile
    {
        public AutoMapperConfig()
        {
            //mapeando as models para as view models e o reverse é pq é um caminho unico 
            CreateMap<Disciplina, DisciplinaViewModel>().ReverseMap();
        }
    }
}
