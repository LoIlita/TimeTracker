using System;
using TimeTracker.Domain.Entities;

namespace TimeTracker.Application.DTOs;

public class ActivitySessionDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public TimeSpan? Duration { get; set; }
    public ActivityProgress Progress { get; set; }
    public int? Rating { get; set; }
    public bool IsActive { get; set; }
}

public class CreateActivitySessionDto
{
    public Guid ProjectId { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public ActivityProgress Progress { get; set; } = ActivityProgress.InProgress;
}

public class UpdateActivitySessionDto
{
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public ActivityProgress Progress { get; set; }
    public int? Rating { get; set; }
} 