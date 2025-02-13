using System;
using TimeTracker.Domain.Validation;

namespace TimeTracker.Domain.Entities;

public class WorkSession
{
    public Guid Id { get; protected set; }
    public DateTime StartTime { get; protected set; }
    public DateTime? EndTime { get; protected set; }
    public string Description { get; protected set; } = string.Empty;
    public string Tags { get; protected set; } = string.Empty;
    public bool IsActive => EndTime == null;

    protected WorkSession() { } // Dla EF Core i klas pochodnych

    public static WorkSession Start(string description = "", string tags = "")
    {
        var session = new WorkSession
        {
            Id = Guid.NewGuid(),
            StartTime = DateTime.Now,
            Description = description,
            Tags = tags
        };

        WorkSessionValidator.ValidateSession(session);
        return session;
    }

    public virtual void Complete()
    {
        if (!IsActive)
        {
            throw new InvalidOperationException("Nie można zakończyć już zakończonej sesji.");
        }

        EndTime = DateTime.Now;
        WorkSessionValidator.ValidateSession(this);
    }

    public virtual void CompleteWithDuration(TimeSpan duration)
    {
        if (!IsActive)
        {
            throw new InvalidOperationException("Nie można zakończyć już zakończonej sesji.");
        }

        EndTime = StartTime.Add(duration);
        WorkSessionValidator.ValidateSession(this);
    }

    public virtual void UpdateDescription(string description)
    {
        Description = description ?? throw new ArgumentNullException(nameof(description));
    }

    public virtual void UpdateTags(string tags)
    {
        Tags = tags ?? string.Empty;
    }

    public virtual void UpdateTimes(DateTime startTime, DateTime? endTime)
    {
        WorkSessionValidator.ValidateStartTime(startTime);
        
        if (endTime.HasValue)
        {
            WorkSessionValidator.ValidateEndTime(startTime, endTime.Value);
        }

        StartTime = startTime;
        EndTime = endTime;
    }

    public TimeSpan? Duration => EndTime.HasValue ? EndTime.Value - StartTime : null;
} 