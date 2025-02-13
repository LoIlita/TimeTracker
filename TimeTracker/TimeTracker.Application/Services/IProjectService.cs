using TimeTracker.Application.DTOs;
using TimeTracker.Domain.Entities;

namespace TimeTracker.Application.Services;

public interface IProjectReader
{
    Task<ProjectDto> GetByIdAsync(Guid id);
    Task<IEnumerable<ProjectDto>> GetAllAsync();
    Task<IEnumerable<ProjectDto>> GetByTypeAsync(ProjectType type);
    Task<ProjectDto> GetWithSessionsAsync(Guid id);
    Task<IEnumerable<ProjectDto>> GetActiveProjectsAsync();
    Task<IEnumerable<ActivitySessionDto>> GetProjectSessionsAsync(Guid projectId, DateTime? startDate = null, DateTime? endDate = null);
}

public interface IProjectWriter
{
    Task<ProjectDto> CreateAsync(CreateProjectDto dto);
    Task<ProjectDto> UpdateAsync(Guid id, UpdateProjectDto dto);
    Task DeleteAsync(Guid id);
}

public interface IProjectStatistics
{
    Task<TimeSpan> GetTotalDurationAsync(Guid projectId);
    Task<double> GetAverageSessionDurationAsync(Guid projectId);
    Task<int> GetTotalSessionsCountAsync(Guid projectId);
    Task<double> GetAverageSessionRatingAsync(Guid projectId);
}

public interface IProjectService : IProjectReader, IProjectWriter, IProjectStatistics
{
    Task<ProjectDto> AddSessionToProjectAsync(Guid projectId, CreateActivitySessionDto sessionDto);
} 