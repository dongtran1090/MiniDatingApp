namespace MiniDatingApp.Models;

public class Like
{
    public int Id { get; set; }
    public int LikerId { get; set; }
    public int LikedId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Profile? Liker { get; set; }
    public Profile? Liked { get; set; }
}