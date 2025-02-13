using TimeTracker.Domain.Entities;

namespace TimeTracker.Domain.Repositories;

public interface IProjectRepository
{
    Task<Project> GetByIdAsync(Guid id);
    Task<IEnumerable<Project>> GetAllAsync();
    Task<IEnumerable<Project>> GetByTypeAsync(ProjectType type);
    Task<Project> GetWithSessionsAsync(Guid id);
    Task<IEnumerable<Project>> GetActiveProjectsAsync();
    Task AddAsync(Project project);
    Task UpdateAsync(Project project);
    Task DeleteAsync(Guid id);
} 