using System.ComponentModel.DataAnnotations;

namespace MiniDatingApp.ViewModels;

public class AvailabilityVm
{
    public int MatchId { get; set; }
    public int ProfileId { get; set; }

    [Required]
    public DateOnly Date { get; set; }

    [Required]
    public TimeOnly From { get; set; }

    [Required]
    public TimeOnly To { get; set; }
}