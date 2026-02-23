namespace MiniDatingApp.Models;

public class DateSuggestion
{
    public int Id { get; set; }
    public int MatchId { get; set; }

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Match? Match { get; set; }
}