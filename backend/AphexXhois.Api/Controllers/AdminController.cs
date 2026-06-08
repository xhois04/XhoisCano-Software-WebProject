using AphexXhois.Api.Data;
using AphexXhois.Api.DTOs;
using AphexXhois.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AphexXhois.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "ADMIN")]
public class AdminController(AppDbContext db) : ControllerBase
{
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await db.Users
            .OrderBy(u => u.Id)
            .Select(u => new AdminUserResponse(
                u.Id, u.Username, u.Email,
                u.Role.ToString(), u.IsBanned, u.CreatedAt))
            .ToListAsync();

        return Ok(users);
    }

    [HttpPost("users/{id:int}/ban")]
    public async Task<IActionResult> Ban(int id)
    {
        var user = await db.Users.FindAsync(id);
        if (user == null) return NotFound();

        user.IsBanned = true;
        await db.SaveChangesAsync();
        return Ok(new { message = $"User {user.Username} banned." });
    }

    [HttpPost("users/{id:int}/unban")]
    public async Task<IActionResult> Unban(int id)
    {
        var user = await db.Users.FindAsync(id);
        if (user == null) return NotFound();

        user.IsBanned = false;
        await db.SaveChangesAsync();
        return Ok(new { message = $"User {user.Username} unbanned." });
    }

    [HttpDelete("reviews/{id:int}")]
    public async Task<IActionResult> DeleteReview(int id)
    {
        var rating = await db.SongRatings.FindAsync(id);
        if (rating != null)
        {
            db.SongRatings.Remove(rating);
            await db.SaveChangesAsync();
            return Ok(new { message = "Rating removed." });
        }

        var comment = await db.Comments.FindAsync(id);
        if (comment != null)
        {
            db.Comments.Remove(comment);
            await db.SaveChangesAsync();
            return Ok(new { message = "Comment removed." });
        }

        return NotFound();
    }

    [HttpPost("users/{id:int}/promote")]
    public async Task<IActionResult> Promote(int id)
    {
        var user = await db.Users.FindAsync(id);
        if (user == null) return NotFound();
        user.Role = UserRole.ADMIN;
        await db.SaveChangesAsync();
        return Ok(new { message = $"{user.Username} promoted to ADMIN." });
    }

    [HttpPost("users/{id:int}/demote")]
    public async Task<IActionResult> Demote(int id)
    {
        var user = await db.Users.FindAsync(id);
        if (user == null) return NotFound();
        user.Role = UserRole.USER;
        await db.SaveChangesAsync();
        return Ok(new { message = $"{user.Username} demoted to USER." });
    }

    [HttpGet("ratings")]
    public async Task<IActionResult> GetAllRatings()
    {
        var ratings = await db.SongRatings
            .Include(r => r.User)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new RatingResponse(
                r.Id, r.SpotifyTrackId, r.TrackName, r.ArtistName,
                r.Rating, r.Review, r.User.Username, r.CreatedAt))
            .ToListAsync();
        return Ok(ratings);
    }

    [HttpGet("comments")]
    public async Task<IActionResult> GetAllComments()
    {
        var comments = await db.Comments
            .Include(c => c.User)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new
            {
                c.Id,
                c.SpotifyTrackId,
                c.Content,
                Username = c.User.Username,
                c.CreatedAt
            })
            .ToListAsync();
        return Ok(comments);
    }

    [HttpDelete("comments/{id:int}")]
    public async Task<IActionResult> DeleteComment(int id)
    {
        var comment = await db.Comments.FindAsync(id);
        if (comment == null) return NotFound();
        db.Comments.Remove(comment);
        await db.SaveChangesAsync();
        return Ok(new { message = "Comment deleted." });
    }

    [HttpDelete("users/{id:int}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await db.Users.FindAsync(id);
        if (user == null) return NotFound();
        db.Users.Remove(user);
        await db.SaveChangesAsync();
        return Ok(new { message = $"User {user.Username} deleted." });
    }
}