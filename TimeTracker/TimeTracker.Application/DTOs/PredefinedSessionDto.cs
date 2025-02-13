using System;

namespace TimeTracker.Application.DTOs;

public class PredefinedSessionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public Guid ProjectId { get; set; }
}

public class CreatePredefinedSessionDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public Guid ProjectId { get; set; }
} 