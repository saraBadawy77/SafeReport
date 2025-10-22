using AutoMapper;
using SafeReport.Application.DTOs;
using SafeReport.Core.Models;


namespace SafeReport.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Incident, IncidentDto>().ReverseMap();
            CreateMap<CreateIncidentDto, Incident>();
            CreateMap<Report, ReportDto>()
         .ForMember(dest => dest.IncidentName, opt => opt.MapFrom(src => src.Incident.NameEn))
         .ForMember(dest => dest.Image, opt => opt.MapFrom(src => src.ImagePath))
         .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address)).ReverseMap();
            CreateMap<IncidentType, IncidentTypeDto>().ReverseMap();

            CreateMap<CreateReportDto, Report>()
                .ForMember(dest => dest.ImagePath, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false));
        }
    }

}
