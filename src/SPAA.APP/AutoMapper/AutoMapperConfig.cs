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
            CreateMap<Turma, TurmaViewModel>().ReverseMap();
            CreateMap<TurmaViewModel, TurmaSalva>()
            .ForMember(dest => dest.CodigoTurmaSalva, opt => opt.Ignore())
            .ForMember(dest => dest.CodigoUnicoTurma, opt => opt.MapFrom(src => src.CodigoTurmaUnico))
            .ForMember(dest => dest.CodigoDisciplina, opt => opt.MapFrom(src => src.CodigoDisciplina))
            .ForMember(dest => dest.Horario, opt => opt.MapFrom(src => src.Horario))
            .ForMember(dest => dest.Matricula, opt => opt.Ignore()); 
        }
    }
}
