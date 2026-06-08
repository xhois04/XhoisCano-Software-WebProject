namespace AphexXhois.Api.Models;

public class Friend
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int FriendId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public User FriendUser { get; set; } = null!;
}