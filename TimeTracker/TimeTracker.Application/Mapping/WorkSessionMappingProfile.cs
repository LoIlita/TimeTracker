using AutoMapper;
using TimeTracker.Application.DTOs;
using TimeTracker.Domain.Entities;

namespace TimeTracker.Application.Mapping;

public class WorkSessionMappingProfile : Profile
{
    public WorkSessionMappingProfile()
    {
        CreateMap<WorkSession, WorkSessionDto>()
            .ForMember(dest => dest.Duration, 
                opt => opt.MapFrom(src => src.Duration))
            .ForMember(dest => dest.ProjectId,
                opt => opt.MapFrom(src => GetProjectId(src)))
            .ForMember(dest => dest.ProjectName,
                opt => opt.MapFrom(src => GetProjectName(src)));

        CreateMap<CreateWorkSessionDto, WorkSession>()
            .ConstructUsing((src, context) =>
            {
                if (src.ProjectId.HasValue)
                {
                    var project = context.Items.TryGetValue("Project", out var projectObj) 
                        ? projectObj as Project 
                        : null;
                        
                    if (project == null)
                        throw new InvalidOperationException("Nie znaleziono projektu do utworzenia sesji.");
                        
                    return ActivitySession.StartForProject(
                        project,
                        "General",
                        src.Description,
                        src.Tags);
                }
                
                return WorkSession.Start(src.Description, src.Tags);
            });
    }

    private static Guid? GetProjectId(WorkSession session)
    {
        if (session is ActivitySession activitySession)
        {
            return activitySession.ProjectId;
        }
        return null;
    }

    private static string? GetProjectName(WorkSession session)
    {
        if (session is ActivitySession activitySession && activitySession.Project != null)
        {
            return activitySession.Project.Name;
        }
        return null;
    }
} 