using AutoMapper;
using SafeReport.Application.DTOs;
using SafeReport.Core.Models;
using System.Globalization;


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
            CreateMap<IncidentType, IncidentTypeDto>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src =>
                CultureInfo.CurrentCulture.TwoLetterISOLanguageName == "ar" ? src.NameAr : src.NameEn))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src =>
                CultureInfo.CurrentCulture.TwoLetterISOLanguageName == "ar" ? src.DescriptionAr : src.DescriptionEn))
            .ForMember(dest => dest.Creationdate, opt => opt.MapFrom(src => src.CreationDate)).ReverseMap();
            CreateMap<CreateIncidentTypeDto, IncidentType>();
        }
    }

}
