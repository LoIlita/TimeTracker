using TimeTracker.Domain.Entities;

namespace TimeTracker.Domain.Repositories;

public interface IPredefinedSessionRepository
{
    Task<PredefinedSession> GetByIdAsync(Guid id);
    Task<IEnumerable<PredefinedSession>> GetByProjectIdAsync(Guid projectId);
    Task AddAsync(PredefinedSession session);
    Task UpdateAsync(PredefinedSession session);
    Task DeleteAsync(Guid id);
} 