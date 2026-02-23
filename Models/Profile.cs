using System.ComponentModel.DataAnnotations;

namespace MiniDatingApp.Models;

public class Profile
{
    public int Id { get; set; }

    [Required, MaxLength(80)]
    public string Name { get; set; } = string.Empty;

    [Range(18, 99)]
    public int Age { get; set; }

    [Required, MaxLength(20)]
    public string Gender { get; set; } = "Other"; 

    [MaxLength(2000)]
    public string Bio { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(120)]
    public string Email { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}