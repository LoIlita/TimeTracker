using System;
using System.Collections.Generic;
using TimeTracker.Domain.Validation;

namespace TimeTracker.Domain.Entities;

public class Project
{
    private readonly List<ActivitySession> _sessions = new();
    
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public ProjectType Type { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastModifiedAt { get; private set; }
    public IReadOnlyCollection<ActivitySession> Sessions => _sessions.AsReadOnly();

    // Dla EF Core
    private Project() { }

    public static Project Create(string name, ProjectType type, string description = "")
    {
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = name,
            Type = type,
            Description = description ?? string.Empty,
            CreatedAt = DateTime.Now
        };

        ProjectValidator.ValidateProject(project);
        return project;
    }

    public void UpdateDetails(string name, string description, ProjectType type)
    {
        Name = name ?? string.Empty;
        Description = description ?? string.Empty;
        Type = type;
        LastModifiedAt = DateTime.Now;
        
        ProjectValidator.ValidateProject(this);
    }

    public void AddSession(ActivitySession session)
    {
        if (session == null) throw new ArgumentNullException(nameof(session));
        
        _sessions.Add(session);
    }

    public ActivitySession? GetLastSessionFromToday()
    {
        var today = DateTime.Today;
        return _sessions
            .Where(s => s.StartTime.Date == today)
            .OrderByDescending(s => s.StartTime)
            .FirstOrDefault();
    }

    public TimeSpan GetTotalDuration()
    {
        return TimeSpan.FromTicks(
            _sessions
                .Where(s => s.EndTime.HasValue)
                .Sum(s => s.Duration?.Ticks ?? 0)
        );
    }
}

public enum ProjectType
{
    Learning,    // Nauka
    Work,        // Praca
    Exercise,    // Ćwiczenia/Sport
    Hobby,       // Hobby
    Reading,     // Czytanie
    Meditation,  // Medytacja
    Other        // Inne
} 