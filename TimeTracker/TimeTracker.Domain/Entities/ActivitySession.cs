using System;
using TimeTracker.Domain.Validation;

namespace TimeTracker.Domain.Entities;

public class ActivitySession : WorkSession
{
    public Guid ProjectId { get; private set; }
    public Project Project { get; private set; } = null!;
    public string Category { get; private set; } = string.Empty;
    public string Notes { get; private set; } = string.Empty;
    public ActivityProgress Progress { get; private set; }
    public int? Rating { get; private set; } // Ocena sesji (1-5)

    private ActivitySession() { } // Dla EF Core

    public static ActivitySession StartForProject(
        Project project,
        string category,
        string description = "",
        string notes = "",
        ActivityProgress progress = ActivityProgress.InProgress)
    {
        if (project == null) throw new ArgumentNullException(nameof(project));

        var session = new ActivitySession
        {
            Id = Guid.NewGuid(),
            ProjectId = project.Id,
            Project = project,
            StartTime = DateTime.Now,
            Description = description,
            Category = category,
            Notes = notes,
            Progress = progress
        };

        ActivitySessionValidator.ValidateSession(session);
        project.AddSession(session);
        return session;
    }

    public void UpdateProgress(ActivityProgress progress)
    {
        Progress = progress;
    }

    public void UpdateNotes(string notes)
    {
        Notes = notes ?? string.Empty;
        ActivitySessionValidator.ValidateSession(this);
    }

    public void UpdateCategory(string category)
    {
        Category = category ?? string.Empty;
        ActivitySessionValidator.ValidateSession(this);
    }

    public void SetRating(int rating)
    {
        if (rating < 1 || rating > 5)
            throw new ArgumentException("Ocena musi być w zakresie 1-5");
            
        Rating = rating;
    }
}

public enum ActivityProgress
{
    NotStarted,
    InProgress,
    Completed,
    NeedsReview,
    Abandoned
} 