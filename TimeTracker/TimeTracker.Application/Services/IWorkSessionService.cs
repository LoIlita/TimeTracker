using TimeTracker.Application.DTOs;

namespace TimeTracker.Application.Services;

public interface IWorkSessionReader
{
    Task<WorkSessionDto> GetActiveSessionAsync();
    Task<IEnumerable<WorkSessionDto>> GetAllAsync();
    Task<IEnumerable<WorkSessionDto>> GetRecentSessionsAsync();
    Task<IEnumerable<string>> GetUniqueHashtags();
}

public interface IWorkSessionWriter
{
    Task<WorkSessionDto> StartSessionAsync(CreateWorkSessionDto dto);
    Task<WorkSessionDto> CompleteCurrentSessionAsync();
    Task<WorkSessionDto> CompleteCurrentSessionWithDurationAsync(TimeSpan duration);
}

public interface IWorkSessionService : IWorkSessionReader, IWorkSessionWriter
{
    Task<WorkSessionDto> GetByIdAsync(Guid id);
    Task<IEnumerable<WorkSessionDto>> GetSessionsByDateRangeAsync(DateTime start, DateTime end);
    Task<WorkSessionDto> UpdateSessionAsync(Guid id, UpdateWorkSessionDto dto);
    Task DeleteSessionAsync(Guid id);
} 