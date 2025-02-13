using AutoMapper;
using TimeTracker.Application.DTOs;
using TimeTracker.Domain.Entities;

namespace TimeTracker.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Project mappings
        CreateMap<Project, ProjectDto>()
            .ForMember(dest => dest.SessionsCount, 
                opt => opt.MapFrom(src => src.Sessions.Count))
            .ForMember(dest => dest.TotalDuration, 
                opt => opt.MapFrom(src => src.GetTotalDuration()));

        CreateMap<CreateProjectDto, Project>()
            .ConstructUsing(src => Project.Create(src.Name, src.Type, src.Description));

        // ActivitySession mappings
        CreateMap<ActivitySession, ActivitySessionDto>()
            .ForMember(dest => dest.ProjectName, 
                opt => opt.MapFrom(src => src.Project.Name))
            .ForMember(dest => dest.Duration, 
                opt => opt.MapFrom(src => src.Duration))
            .ForMember(dest => dest.IsActive, 
                opt => opt.MapFrom(src => src.IsActive));

        CreateMap<CreateActivitySessionDto, ActivitySession>();
        CreateMap<UpdateActivitySessionDto, ActivitySession>();
    }
} 