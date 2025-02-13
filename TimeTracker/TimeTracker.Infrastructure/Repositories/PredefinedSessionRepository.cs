using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TimeTracker.Domain.Entities;
using TimeTracker.Domain.Repositories;
using TimeTracker.Infrastructure.Data;

namespace TimeTracker.Infrastructure.Repositories;

public class PredefinedSessionRepository : IPredefinedSessionRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<PredefinedSessionRepository> _logger;

    public PredefinedSessionRepository(AppDbContext context, ILogger<PredefinedSessionRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PredefinedSession> GetByIdAsync(Guid id)
    {
        try
        {
            _logger.LogDebug("Pobieranie predefiniowanej sesji o ID: {Id}", id);
            var session = await _context.PredefinedSessions
                .Include(s => s.Project)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (session == null)
            {
                _logger.LogWarning("Nie znaleziono predefiniowanej sesji o ID: {Id}", id);
                throw new KeyNotFoundException($"Nie znaleziono predefiniowanej sesji o ID: {id}");
            }

            return session;
        }
        catch (Exception ex) when (ex is not KeyNotFoundException)
        {
            _logger.LogError(ex, "Błąd podczas pobierania predefiniowanej sesji o ID: {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<PredefinedSession>> GetByProjectIdAsync(Guid projectId)
    {
        try
        {
            _logger.LogDebug("Pobieranie predefiniowanych sesji dla projektu: {ProjectId}", projectId);
            return await _context.PredefinedSessions
                .Include(s => s.Project)
                .Where(s => s.ProjectId == projectId)
                .OrderBy(s => s.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas pobierania predefiniowanych sesji dla projektu: {ProjectId}", projectId);
            throw;
        }
    }

    public async Task AddAsync(PredefinedSession session)
    {
        try
        {
            _logger.LogDebug("Dodawanie nowej predefiniowanej sesji: {Name}", session.Name);
            await _context.PredefinedSessions.AddAsync(session);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Predefiniowana sesja dodana pomyślnie, ID: {Id}", session.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas dodawania predefiniowanej sesji: {Name}", session.Name);
            throw;
        }
    }

    public async Task UpdateAsync(PredefinedSession session)
    {
        try
        {
            _logger.LogDebug("Aktualizacja predefiniowanej sesji o ID: {Id}", session.Id);
            _context.PredefinedSessions.Update(session);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Predefiniowana sesja zaktualizowana pomyślnie");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas aktualizacji predefiniowanej sesji o ID: {Id}", session.Id);
            throw;
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        try
        {
            _logger.LogDebug("Usuwanie predefiniowanej sesji o ID: {Id}", id);
            var session = await GetByIdAsync(id);
            _context.PredefinedSessions.Remove(session);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Predefiniowana sesja usunięta pomyślnie");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas usuwania predefiniowanej sesji o ID: {Id}", id);
            throw;
        }
    }
} 