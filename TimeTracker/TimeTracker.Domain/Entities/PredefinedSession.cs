using System;

namespace TimeTracker.Domain.Entities;

public class PredefinedSession
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;
    public Guid ProjectId { get; private set; }
    public Project Project { get; private set; } = null!;

    private PredefinedSession() { } // Dla EF Core

    public static PredefinedSession Create(
        string name,
        string description,
        string category,
        Project project)
    {
        if (project == null) throw new ArgumentNullException(nameof(project));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Nazwa nie może być pusta", nameof(name));

        return new PredefinedSession
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description ?? string.Empty,
            Category = category ?? string.Empty,
            ProjectId = project.Id,
            Project = project
        };
    }

    public void Update(string name, string description, string category)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Nazwa nie może być pusta", nameof(name));

        Name = name;
        Description = description ?? string.Empty;
        Category = category ?? string.Empty;
    }
} 