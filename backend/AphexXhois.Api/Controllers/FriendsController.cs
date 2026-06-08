using System.Security.Claims;
using AphexXhois.Api.Data;
using AphexXhois.Api.DTOs;
using AphexXhois.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AphexXhois.Api.Controllers;

[ApiController]
[Route("api/friends")]
[Authorize]
public class FriendsController(AppDbContext db) : ControllerBase
{
    private int MyId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetFriends()
    {
        var friends = await db.Friends
            .Where(f => f.UserId == MyId)
            .Include(f => f.FriendUser)
            .Select(f => new UserSearchResponse(f.FriendUser.Id, f.FriendUser.Username, true))
            .ToListAsync();

        return Ok(friends);
    }

    [HttpPost("{friendId:int}")]
    public async Task<IActionResult> AddFriend(int friendId)
    {
        if (friendId == MyId)
            return BadRequest(new { message = "Cannot add yourself." });

        if (!await db.Users.AnyAsync(u => u.Id == friendId))
            return NotFound(new { message = "User not found." });

        if (await db.Friends.AnyAsync(f => f.UserId == MyId && f.FriendId == friendId))
            return Conflict(new { message = "Already friends." });

        db.Friends.Add(new Friend { UserId = MyId, FriendId = friendId });
        await db.SaveChangesAsync();
        return Ok(new { message = "Friend added." });
    }

    [HttpDelete("{friendId:int}")]
    public async Task<IActionResult> RemoveFriend(int friendId)
    {
        var entry = await db.Friends.FirstOrDefaultAsync(f => f.UserId == MyId && f.FriendId == friendId);
        if (entry == null) return NotFound(new { message = "Not in friends list." });

        db.Friends.Remove(entry);
        await db.SaveChangesAsync();
        return Ok(new { message = "Friend removed." });
    }

    [HttpGet("activity")]
    public async Task<IActionResult> GetFriendActivity()
    {
        var friendIds = await db.Friends
            .Where(f => f.UserId == MyId)
            .Select(f => f.FriendId)
            .ToListAsync();

        var activity = await db.SongRatings
            .Where(r => friendIds.Contains(r.UserId))
            .Include(r => r.User)
            .OrderByDescending(r => r.CreatedAt)
            .Take(50)
            .Select(r => new RatingResponse(
                r.Id, r.SpotifyTrackId, r.TrackName, r.ArtistName,
                r.Rating, r.Review, r.User.Username, r.CreatedAt))
            .ToListAsync();

        return Ok(activity);
    }
}