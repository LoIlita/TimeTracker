using TimeTracker.Application.DTOs;

namespace TimeTracker.Application.Services;

public interface IPredefinedSessionService
{
    Task<IEnumerable<PredefinedSessionDto>> GetByProjectIdAsync(Guid projectId);
    Task<PredefinedSessionDto> CreateAsync(CreatePredefinedSessionDto dto);
    Task<PredefinedSessionDto> UpdateAsync(Guid id, CreatePredefinedSessionDto dto);
    Task DeleteAsync(Guid id);
} 