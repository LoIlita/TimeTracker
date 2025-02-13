namespace TimeTracker.Application.DTOs;

public class CreateWorkSessionDto
{
    public string Description { get; set; } = string.Empty;
    public string Tags { get; set; } = string.Empty;
    public Guid? ProjectId { get; set; }
} 