using AutoMapper;
using Microsoft.Extensions.Logging;
using TimeTracker.Application.DTOs;
using TimeTracker.Domain.Entities;
using TimeTracker.Domain.Repositories;

namespace TimeTracker.Application.Services;

public class PredefinedSessionService : IPredefinedSessionService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IPredefinedSessionRepository _sessionRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<PredefinedSessionService> _logger;

    public PredefinedSessionService(
        IProjectRepository projectRepository,
        IPredefinedSessionRepository sessionRepository,
        IMapper mapper,
        ILogger<PredefinedSessionService> logger)
    {
        _projectRepository = projectRepository;
        _sessionRepository = sessionRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<PredefinedSessionDto>> GetByProjectIdAsync(Guid projectId)
    {
        try
        {
            _logger.LogInformation("Pobieranie predefiniowanych sesji dla projektu: {ProjectId}", projectId);
            var sessions = await _sessionRepository.GetByProjectIdAsync(projectId);
            return _mapper.Map<IEnumerable<PredefinedSessionDto>>(sessions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas pobierania predefiniowanych sesji");
            throw;
        }
    }

    public async Task<PredefinedSessionDto> CreateAsync(CreatePredefinedSessionDto dto)
    {
        try
        {
            _logger.LogInformation("Tworzenie nowej predefiniowanej sesji dla projektu: {ProjectId}", dto.ProjectId);
            
            var project = await _projectRepository.GetByIdAsync(dto.ProjectId);
            var session = PredefinedSession.Create(dto.Name, dto.Description, dto.Category, project);
            
            await _sessionRepository.AddAsync(session);
            return _mapper.Map<PredefinedSessionDto>(session);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas tworzenia predefiniowanej sesji");
            throw;
        }
    }

    public async Task<PredefinedSessionDto> UpdateAsync(Guid id, CreatePredefinedSessionDto dto)
    {
        try
        {
            _logger.LogInformation("Aktualizacja predefiniowanej sesji: {Id}", id);
            
            var session = await _sessionRepository.GetByIdAsync(id);
            session.Update(dto.Name, dto.Description, dto.Category);
            
            await _sessionRepository.UpdateAsync(session);
            return _mapper.Map<PredefinedSessionDto>(session);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas aktualizacji predefiniowanej sesji");
            throw;
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Usuwanie predefiniowanej sesji: {Id}", id);
            await _sessionRepository.DeleteAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas usuwania predefiniowanej sesji");
            throw;
        }
    }
} 