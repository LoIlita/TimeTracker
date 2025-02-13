namespace TimeTracker.Application.DTOs;

public class UpdateWorkSessionDto
{
    public required string Description { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string Tags { get; set; } = string.Empty;
} 