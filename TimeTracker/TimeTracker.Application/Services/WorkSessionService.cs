using AutoMapper;
using TimeTracker.Application.DTOs;
using TimeTracker.Domain.Entities;
using TimeTracker.Domain.Repositories;

namespace TimeTracker.Application.Services;

public class WorkSessionService : IWorkSessionService
{
    private readonly IWorkSessionRepository _repository;
    private readonly IProjectRepository _projectRepository;
    private readonly IMapper _mapper;

    public WorkSessionService(
        IWorkSessionRepository repository, 
        IProjectRepository projectRepository,
        IMapper mapper)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<WorkSessionDto> StartSessionAsync(CreateWorkSessionDto dto)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));
        
        var activeSession = await _repository.GetActiveSessionAsync();
        if (activeSession != null)
        {
            throw new InvalidOperationException("Nie można rozpocząć nowej sesji, gdy inna jest aktywna.");
        }

        WorkSession session;
        if (dto.ProjectId.HasValue)
        {
            var project = await _projectRepository.GetByIdAsync(dto.ProjectId.Value);
            session = _mapper.Map<WorkSession>(dto, opt => opt.Items["Project"] = project);
        }
        else
        {
            session = WorkSession.Start(dto.Description, dto.Tags);
        }

        await _repository.AddAsync(session);
        return _mapper.Map<WorkSessionDto>(session);
    }

    public async Task<WorkSessionDto> CompleteCurrentSessionAsync()
    {
        var activeSession = await _repository.GetActiveSessionAsync();
        if (activeSession == null)
        {
            throw new InvalidOperationException("Nie znaleziono aktywnej sesji do zakończenia.");
        }

        activeSession.Complete();
        await _repository.UpdateAsync(activeSession);

        return _mapper.Map<WorkSessionDto>(activeSession);
    }

    public async Task<WorkSessionDto> CompleteCurrentSessionWithDurationAsync(TimeSpan duration)
    {
        var activeSession = await _repository.GetActiveSessionAsync();
        if (activeSession == null)
        {
            throw new InvalidOperationException("Nie znaleziono aktywnej sesji do zakończenia.");
        }

        System.Diagnostics.Debug.WriteLine($"[WorkSessionService] Znaleziono aktywną sesję: {activeSession.Id}");
        System.Diagnostics.Debug.WriteLine($"[WorkSessionService] Czas trwania: {duration}");
        System.Diagnostics.Debug.WriteLine($"[WorkSessionService] Czas rozpoczęcia: {activeSession.StartTime}");

        activeSession.CompleteWithDuration(duration);
        
        System.Diagnostics.Debug.WriteLine($"[WorkSessionService] Czas zakończenia po aktualizacji: {activeSession.EndTime}");
        
        await _repository.UpdateAsync(activeSession);
        System.Diagnostics.Debug.WriteLine("[WorkSessionService] Sesja zaktualizowana w bazie danych");

        var result = _mapper.Map<WorkSessionDto>(activeSession);
        System.Diagnostics.Debug.WriteLine($"[WorkSessionService] Zmapowano na DTO, czas zakończenia: {result.EndTime}");
        
        return result;
    }

    public async Task<WorkSessionDto> GetActiveSessionAsync()
    {
        var activeSession = await _repository.GetActiveSessionAsync();
        return activeSession == null ? null! : _mapper.Map<WorkSessionDto>(activeSession);
    }

    public async Task<WorkSessionDto> GetByIdAsync(Guid id)
    {
        var session = await _repository.GetByIdAsync(id);
        if (session == null)
        {
            throw new KeyNotFoundException($"Nie znaleziono sesji o ID: {id}");
        }

        return _mapper.Map<WorkSessionDto>(session);
    }

    public async Task<IEnumerable<WorkSessionDto>> GetAllAsync()
    {
        var sessions = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<WorkSessionDto>>(sessions);
    }

    public async Task<IEnumerable<WorkSessionDto>> GetRecentSessionsAsync()
    {
        var sessions = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<WorkSessionDto>>(sessions.OrderByDescending(s => s.StartTime));
    }

    public async Task<IEnumerable<string>> GetUniqueHashtags()
    {
        var sessions = await GetAllAsync();
        return sessions
            .SelectMany(s => s.HashtagList)
            .Distinct()
            .OrderBy(h => h)
            .ToList();
    }

    public async Task<IEnumerable<WorkSessionDto>> GetSessionsByDateRangeAsync(DateTime start, DateTime end)
    {
        var sessions = await _repository.GetSessionsByDateRangeAsync(start, end);
        return _mapper.Map<IEnumerable<WorkSessionDto>>(sessions);
    }

    public async Task<WorkSessionDto> UpdateSessionAsync(Guid id, UpdateWorkSessionDto dto)
    {
        var session = await _repository.GetByIdAsync(id);
        if (session == null)
        {
            throw new KeyNotFoundException($"Nie znaleziono sesji o ID: {id}");
        }

        session.UpdateDescription(dto.Description);
        session.UpdateTimes(dto.StartTime, dto.EndTime);

        await _repository.UpdateAsync(session);

        return _mapper.Map<WorkSessionDto>(session);
    }

    public async Task DeleteSessionAsync(Guid id)
    {
        try
        {
            var session = await _repository.GetByIdAsync(id);
            if (session == null)
            {
                throw new KeyNotFoundException($"Nie znaleziono sesji o ID: {id}");
            }

            if (session.IsActive)
            {
                throw new InvalidOperationException("Nie można usunąć aktywnej sesji.");
            }

            await _repository.DeleteAsync(id);
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"Błąd podczas usuwania sesji: {ex.Message}", ex);
        }
    }
} 