namespace MiniDatingApp.Models;

public class Match
{
    public int Id { get; set; }

    // normalized: UserLowId < UserHighId
    public int UserLowId { get; set; }
    public int UserHighId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Profile? UserLow { get; set; }
    public Profile? UserHigh { get; set; }

    public DateSuggestion? Suggestion { get; set; }
}