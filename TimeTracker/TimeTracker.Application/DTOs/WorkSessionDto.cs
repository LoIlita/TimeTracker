namespace TimeTracker.Application.DTOs;

public class WorkSessionDto
{
    public Guid Id { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public required string Description { get; set; }
    public string Tags { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public TimeSpan? Duration { get; set; }
    public Guid? ProjectId { get; set; }
    public string? ProjectName { get; set; }

    public IEnumerable<string> HashtagList 
    { 
        get
        {
            if (string.IsNullOrEmpty(Tags)) 
                return Enumerable.Empty<string>();
            
            return Tags.Split(' ')
                .Where(w => w.StartsWith("#"))
                .Select(w => w.Trim())
                .Distinct();
        }
    }
} 