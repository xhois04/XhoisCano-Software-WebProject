namespace AphexXhois.Api.Models;

public class SongRating
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string SpotifyTrackId { get; set; } = string.Empty;
    public string TrackName { get; set; } = string.Empty;
    public string ArtistName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Review { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}