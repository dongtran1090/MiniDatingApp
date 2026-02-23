namespace MiniDatingApp.Models;

public class AvailabilitySlot
{
    public int Id { get; set; }

    public int MatchId { get; set; }
    public int ProfileId { get; set; }

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Match? Match { get; set; }
    public Profile? Profile { get; set; }
}