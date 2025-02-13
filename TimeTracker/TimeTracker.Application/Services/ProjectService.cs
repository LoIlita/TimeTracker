using AutoMapper;
using Microsoft.Extensions.Logging;
using TimeTracker.Application.DTOs;
using TimeTracker.Domain.Entities;
using TimeTracker.Domain.Repositories;
using TimeTracker.Domain.Validation;

namespace TimeTracker.Application.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IWorkSessionRepository _sessionRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ProjectService> _logger;

    public ProjectService(
        IProjectRepository projectRepository,
        IWorkSessionRepository sessionRepository,
        IMapper mapper,
        ILogger<ProjectService> logger)
    {
        _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        _sessionRepository = sessionRepository ?? throw new ArgumentNullException(nameof(sessionRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ProjectDto> GetByIdAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Pobieranie projektu o ID: {Id}", id);
            var project = await _projectRepository.GetByIdAsync(id);
            return _mapper.Map<ProjectDto>(project);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas pobierania projektu o ID: {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<ProjectDto>> GetAllAsync()
    {
        try
        {
            _logger.LogInformation("Pobieranie wszystkich projektów");
            var projects = await _projectRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<ProjectDto>>(projects);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas pobierania wszystkich projektów");
            throw;
        }
    }

    public async Task<IEnumerable<ProjectDto>> GetByTypeAsync(ProjectType type)
    {
        try
        {
            _logger.LogInformation("Pobieranie projektów typu: {Type}", type);
            var projects = await _projectRepository.GetByTypeAsync(type);
            return _mapper.Map<IEnumerable<ProjectDto>>(projects);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas pobierania projektów typu: {Type}", type);
            throw;
        }
    }

    public async Task<ProjectDto> GetWithSessionsAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Pobieranie projektu z sesjami, ID: {Id}", id);
            var project = await _projectRepository.GetWithSessionsAsync(id);
            return _mapper.Map<ProjectDto>(project);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas pobierania projektu z sesjami, ID: {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<ProjectDto>> GetActiveProjectsAsync()
    {
        try
        {
            _logger.LogInformation("Pobieranie aktywnych projektów");
            var projects = await _projectRepository.GetActiveProjectsAsync();
            return _mapper.Map<IEnumerable<ProjectDto>>(projects);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas pobierania aktywnych projektów");
            throw;
        }
    }

    public async Task<ProjectDto> CreateAsync(CreateProjectDto dto)
    {
        try
        {
            _logger.LogInformation("Tworzenie nowego projektu: {Name}", dto.Name);
            
            var project = Project.Create(dto.Name, dto.Type, dto.Description);
            await _projectRepository.AddAsync(project);
            
            _logger.LogInformation("Projekt utworzony pomyślnie, ID: {Id}", project.Id);
            return _mapper.Map<ProjectDto>(project);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Błąd walidacji podczas tworzenia projektu: {Message}", ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas tworzenia projektu");
            throw;
        }
    }

    public async Task<ProjectDto> UpdateAsync(Guid id, UpdateProjectDto dto)
    {
        try
        {
            _logger.LogInformation("Aktualizacja projektu o ID: {Id}", id);
            
            var project = await _projectRepository.GetByIdAsync(id);
            project.UpdateDetails(dto.Name, dto.Description, dto.Type);
            await _projectRepository.UpdateAsync(project);
            
            _logger.LogInformation("Projekt zaktualizowany pomyślnie");
            return _mapper.Map<ProjectDto>(project);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Błąd walidacji podczas aktualizacji projektu: {Message}", ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas aktualizacji projektu o ID: {Id}", id);
            throw;
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Usuwanie projektu o ID: {Id}", id);
            await _projectRepository.DeleteAsync(id);
            _logger.LogInformation("Projekt usunięty pomyślnie");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas usuwania projektu o ID: {Id}", id);
            throw;
        }
    }

    public async Task<TimeSpan> GetTotalDurationAsync(Guid projectId)
    {
        try
        {
            _logger.LogInformation("Obliczanie całkowitego czasu dla projektu o ID: {Id}", projectId);
            var project = await _projectRepository.GetWithSessionsAsync(projectId);
            return project.GetTotalDuration();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas obliczania całkowitego czasu dla projektu o ID: {Id}", projectId);
            throw;
        }
    }

    public async Task<double> GetAverageSessionDurationAsync(Guid projectId)
    {
        try
        {
            _logger.LogInformation("Obliczanie średniego czasu sesji dla projektu o ID: {Id}", projectId);
            var project = await _projectRepository.GetWithSessionsAsync(projectId);
            var totalDuration = project.GetTotalDuration();
            var sessionsCount = project.Sessions.Count;
            
            return sessionsCount > 0 
                ? totalDuration.TotalMinutes / sessionsCount 
                : 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas obliczania średniego czasu sesji dla projektu o ID: {Id}", projectId);
            throw;
        }
    }

    public async Task<int> GetTotalSessionsCountAsync(Guid projectId)
    {
        try
        {
            _logger.LogInformation("Pobieranie liczby sesji dla projektu o ID: {Id}", projectId);
            var project = await _projectRepository.GetWithSessionsAsync(projectId);
            return project.Sessions.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas pobierania liczby sesji dla projektu o ID: {Id}", projectId);
            throw;
        }
    }

    public async Task<double> GetAverageSessionRatingAsync(Guid projectId)
    {
        try
        {
            _logger.LogInformation("Obliczanie średniej oceny sesji dla projektu o ID: {Id}", projectId);
            var project = await _projectRepository.GetWithSessionsAsync(projectId);
            var ratedSessions = project.Sessions
                .OfType<ActivitySession>()
                .Where(s => s.Rating.HasValue);
            
            return ratedSessions.Any() 
                ? ratedSessions.Average(s => s.Rating!.Value) 
                : 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas obliczania średniej oceny sesji dla projektu o ID: {Id}", projectId);
            throw;
        }
    }

    public async Task<ProjectDto> AddSessionToProjectAsync(Guid projectId, CreateActivitySessionDto sessionDto)
    {
        try
        {
            _logger.LogInformation("Dodawanie nowej sesji do projektu o ID: {Id}", projectId);
            
            var project = await _projectRepository.GetByIdAsync(projectId);
            var session = ActivitySession.StartForProject(
                project,
                sessionDto.Category,
                sessionDto.Description,
                sessionDto.Notes,
                sessionDto.Progress);
            
            await _sessionRepository.AddAsync(session);
            
            _logger.LogInformation("Sesja dodana pomyślnie do projektu");
            return await GetWithSessionsAsync(projectId);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Błąd walidacji podczas dodawania sesji: {Message}", ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas dodawania sesji do projektu o ID: {Id}", projectId);
            throw;
        }
    }

    public async Task<IEnumerable<ActivitySessionDto>> GetProjectSessionsAsync(Guid projectId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            _logger.LogInformation("Pobieranie sesji dla projektu o ID: {Id}", projectId);
            
            var project = await _projectRepository.GetWithSessionsAsync(projectId);
            var sessions = project.Sessions
                .OfType<ActivitySession>()
                .Where(s => (!startDate.HasValue || s.StartTime >= startDate.Value) &&
                           (!endDate.HasValue || s.EndTime <= endDate.Value))
                .OrderByDescending(s => s.StartTime);
            
            return _mapper.Map<IEnumerable<ActivitySessionDto>>(sessions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas pobierania sesji dla projektu o ID: {Id}", projectId);
            throw;
        }
    }
} 