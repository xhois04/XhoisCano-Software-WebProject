namespace AphexXhois.Api.Models;

public enum UserRole { USER, ADMIN }

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.USER;
    public bool IsBanned { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? ResetToken { get; set; }
    public DateTime? ResetTokenExpiry { get; set; }

    public ICollection<Friend> FriendsAdded { get; set; } = [];
    public ICollection<SongRating> SongRatings { get; set; } = [];
    public ICollection<Comment> Comments { get; set; } = [];
}