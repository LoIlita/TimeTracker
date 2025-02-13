using System;
using System.Collections.Generic;
using TimeTracker.Domain.Entities;

namespace TimeTracker.Application.DTOs;

public class ProjectDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ProjectType Type { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public TimeSpan TotalDuration { get; set; }
    public int SessionsCount { get; set; }
    public ICollection<ActivitySessionDto> Sessions { get; set; } = new List<ActivitySessionDto>();
}

public class CreateProjectDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ProjectType Type { get; set; }
}

public class UpdateProjectDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ProjectType Type { get; set; }
} 