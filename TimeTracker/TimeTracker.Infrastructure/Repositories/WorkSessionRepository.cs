using Microsoft.EntityFrameworkCore;
using TimeTracker.Domain.Entities;
using TimeTracker.Domain.Repositories;
using TimeTracker.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace TimeTracker.Infrastructure.Repositories;

public class WorkSessionRepository : IWorkSessionRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<WorkSessionRepository> _logger;

    public WorkSessionRepository(AppDbContext context, ILogger<WorkSessionRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<WorkSession> GetByIdAsync(Guid id)
    {
        var session = await _context.WorkSessions.FindAsync(id);
        if (session == null)
        {
            throw new KeyNotFoundException($"Nie znaleziono sesji o ID: {id}");
        }
        return session;
    }

    public async Task<IEnumerable<WorkSession>> GetAllAsync()
    {
        return await _context.WorkSessions.ToListAsync();
    }

    public async Task<WorkSession?> GetActiveSessionAsync()
    {
        var session = await _context.WorkSessions
            .FirstOrDefaultAsync(s => s.EndTime == null);
            
        System.Diagnostics.Debug.WriteLine($"[WorkSessionRepository] GetActiveSessionAsync - Znaleziono sesję: {session?.Id}");
        return session;
    }

    public async Task<IEnumerable<WorkSession>> GetSessionsByDateRangeAsync(DateTime start, DateTime end)
    {
        return await _context.WorkSessions
            .Where(s => s.StartTime >= start && (s.EndTime == null || s.EndTime <= end))
            .ToListAsync();
    }

    public async Task AddAsync(WorkSession session)
    {
        try
        {
            _logger.LogInformation("Dodawanie nowej sesji do bazy - ID: {Id}, Opis: {Description}, Tags: {Tags}", 
                session.Id, session.Description, session.Tags);
                
            await _context.WorkSessions.AddAsync(session);
            await _context.SaveChangesAsync();
            
            // Sprawdź, czy sesja została zapisana
            var savedSession = await _context.WorkSessions.FindAsync(session.Id);
            if (savedSession != null)
            {
                _logger.LogInformation("Sesja zapisana pomyślnie - ID: {Id}, StartTime: {StartTime}", 
                    savedSession.Id, savedSession.StartTime);
            }
            else
            {
                _logger.LogWarning("Nie można znaleźć zapisanej sesji po zapisie - ID: {Id}", session.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas dodawania sesji - ID: {Id}", session.Id);
            throw;
        }
    }

    public async Task UpdateAsync(WorkSession session)
    {
        System.Diagnostics.Debug.WriteLine($"[WorkSessionRepository] UpdateAsync - Aktualizacja sesji: {session.Id}");
        System.Diagnostics.Debug.WriteLine($"[WorkSessionRepository] UpdateAsync - EndTime: {session.EndTime}");
        
        _context.WorkSessions.Update(session);
        await _context.SaveChangesAsync();
        
        System.Diagnostics.Debug.WriteLine("[WorkSessionRepository] UpdateAsync - Zmiany zapisane w bazie danych");
    }
} 