using System.Security.Claims;
using AphexXhois.Api.Data;
using AphexXhois.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AphexXhois.Api.Controllers;

[ApiController]
[Route("api/comments")]
public class CommentsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetComments([FromQuery] string trackId)
    {
        if (string.IsNullOrWhiteSpace(trackId))
            return BadRequest(new { message = "trackId required." });

        var comments = await db.Comments
            .Where(c => c.SpotifyTrackId == trackId)
            .Include(c => c.User)
            .OrderBy(c => c.CreatedAt)
            .Select(c => new
            {
                c.Id,
                c.Content,
                Username = c.User.Username,
                CreatedAt = c.CreatedAt.ToString("yyyy-MM-dd HH:mm")
            })
            .ToListAsync();

        return Ok(comments);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> PostComment([FromBody] PostCommentRequest req)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var comment = new Comment
        {
            UserId = userId,
            SpotifyTrackId = req.TrackId,
            Content = req.Content
        };

        db.Comments.Add(comment);
        await db.SaveChangesAsync();

        var user = await db.Users.FindAsync(userId);
        return Ok(new
        {
            comment.Id,
            comment.Content,
            Username = user!.Username,
            CreatedAt = comment.CreatedAt.ToString("yyyy-MM-dd HH:mm")
        });
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> DeleteComment(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var role = User.FindFirstValue(ClaimTypes.Role);

        var comment = await db.Comments.FindAsync(id);
        if (comment == null) return NotFound();

        if (comment.UserId != userId && role != "ADMIN")
            return Forbid();

        db.Comments.Remove(comment);
        await db.SaveChangesAsync();
        return Ok(new { message = "Deleted." });
    }
}

public record PostCommentRequest(string TrackId, string Content);