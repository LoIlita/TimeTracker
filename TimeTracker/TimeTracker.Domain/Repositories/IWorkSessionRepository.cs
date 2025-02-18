using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TimeTracker.Domain.Entities;

namespace TimeTracker.Domain.Repositories;

public interface IWorkSessionRepository
{
    Task<WorkSession> GetByIdAsync(Guid id);
    Task<IEnumerable<WorkSession>> GetAllAsync();
    Task<WorkSession?> GetActiveSessionAsync();
    Task AddAsync(WorkSession session);
    Task UpdateAsync(WorkSession session);
    Task<IEnumerable<WorkSession>> GetSessionsByDateRangeAsync(DateTime start, DateTime end);
    Task DeleteAsync(Guid id);
} 