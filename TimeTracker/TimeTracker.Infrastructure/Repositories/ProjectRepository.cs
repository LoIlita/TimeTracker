using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TimeTracker.Domain.Entities;
using TimeTracker.Domain.Repositories;
using TimeTracker.Infrastructure.Data;

namespace TimeTracker.Infrastructure.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<ProjectRepository> _logger;

    public ProjectRepository(AppDbContext context, ILogger<ProjectRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Project> GetByIdAsync(Guid id)
    {
        try
        {
            _logger.LogDebug("Pobieranie projektu o ID: {Id}", id);
            var project = await _context.Projects.FindAsync(id);
            
            if (project == null)
            {
                _logger.LogWarning("Nie znaleziono projektu o ID: {Id}", id);
                throw new KeyNotFoundException($"Nie znaleziono projektu o ID: {id}");
            }
            
            return project;
        }
        catch (Exception ex) when (ex is not KeyNotFoundException)
        {
            _logger.LogError(ex, "Błąd podczas pobierania projektu o ID: {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Project>> GetAllAsync()
    {
        try
        {
            _logger.LogDebug("Pobieranie wszystkich projektów");
            return await _context.Projects
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas pobierania wszystkich projektów");
            throw;
        }
    }

    public async Task<IEnumerable<Project>> GetByTypeAsync(ProjectType type)
    {
        try
        {
            _logger.LogDebug("Pobieranie projektów typu: {Type}", type);
            return await _context.Projects
                .Where(p => p.Type == type)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas pobierania projektów typu: {Type}", type);
            throw;
        }
    }

    public async Task<Project> GetWithSessionsAsync(Guid id)
    {
        try
        {
            _logger.LogDebug("Pobieranie projektu z sesjami, ID: {Id}", id);
            var project = await _context.Projects
                .Include(p => p.Sessions)
                    .ThenInclude(s => s.Project)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                _logger.LogWarning("Nie znaleziono projektu o ID: {Id}", id);
                throw new KeyNotFoundException($"Nie znaleziono projektu o ID: {id}");
            }

            return project;
        }
        catch (Exception ex) when (ex is not KeyNotFoundException)
        {
            _logger.LogError(ex, "Błąd podczas pobierania projektu z sesjami, ID: {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Project>> GetActiveProjectsAsync()
    {
        try
        {
            _logger.LogDebug("Pobieranie aktywnych projektów");
            var thirtyDaysAgo = DateTime.Now.AddDays(-30);
            
            return await _context.Projects
                .Include(p => p.Sessions)
                .Where(p => p.Sessions.Any(s => s.StartTime >= thirtyDaysAgo))
                .OrderByDescending(p => p.Sessions.Max(s => s.StartTime))
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas pobierania aktywnych projektów");
            throw;
        }
    }

    public async Task AddAsync(Project project)
    {
        try
        {
            _logger.LogDebug("Dodawanie nowego projektu: {Name}", project.Name);
            await _context.Projects.AddAsync(project);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Projekt dodany pomyślnie, ID: {Id}", project.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas dodawania projektu: {Name}", project.Name);
            throw;
        }
    }

    public async Task UpdateAsync(Project project)
    {
        try
        {
            _logger.LogDebug("Aktualizacja projektu o ID: {Id}", project.Id);
            _context.Projects.Update(project);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Projekt zaktualizowany pomyślnie");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas aktualizacji projektu o ID: {Id}", project.Id);
            throw;
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        try
        {
            _logger.LogDebug("Usuwanie projektu o ID: {Id}", id);
            var project = await GetByIdAsync(id);
            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Projekt usunięty pomyślnie");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas usuwania projektu o ID: {Id}", id);
            throw;
        }
    }
} 