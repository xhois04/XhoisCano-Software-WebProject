namespace AphexXhois.Api.Models;

public class Comment
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string SpotifyTrackId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}