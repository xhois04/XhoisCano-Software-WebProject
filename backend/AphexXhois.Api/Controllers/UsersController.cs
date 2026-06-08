using System.Security.Claims;
using AphexXhois.Api.Data;
using AphexXhois.Api.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AphexXhois.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController(AppDbContext db) : ControllerBase
{
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await db.Users.FindAsync(userId);
        if (user == null) return NotFound();

        return Ok(new UserProfileResponse(
            user.Id, user.Username, user.Email,
            user.Role.ToString(), user.IsBanned, user.CreatedAt));
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q)
    {
        var meId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        if (string.IsNullOrWhiteSpace(q))
            return BadRequest(new { message = "Query required." });

        var myFriendIds = await db.Friends
            .Where(f => f.UserId == meId)
            .Select(f => f.FriendId)
            .ToListAsync();

        var users = await db.Users
            .Where(u => u.Id != meId && u.Username.ToLower().Contains(q.ToLower()))
            .Take(20)
            .Select(u => new UserSearchResponse(u.Id, u.Username, myFriendIds.Contains(u.Id)))
            .ToListAsync();

        return Ok(users);
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile(UpdateProfileRequest req)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await db.Users.FindAsync(userId);
        if (user == null) return NotFound();

        if (!string.IsNullOrWhiteSpace(req.Username) && req.Username != user.Username)
        {
            if (await db.Users.AnyAsync(u => u.Username == req.Username && u.Id != userId))
                return Conflict(new { message = "Username already taken." });
            user.Username = req.Username;
        }

        if (!string.IsNullOrWhiteSpace(req.Email) && req.Email != user.Email)
        {
            if (await db.Users.AnyAsync(u => u.Email == req.Email && u.Id != userId))
                return Conflict(new { message = "Email already in use." });
            user.Email = req.Email;
        }

        await db.SaveChangesAsync();
        return Ok(new UserProfileResponse(user.Id, user.Username, user.Email,
            user.Role.ToString(), user.IsBanned, user.CreatedAt));
    }

    [HttpPut("me/password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest req)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await db.Users.FindAsync(userId);
        if (user == null) return NotFound();

        if (!BCrypt.Net.BCrypt.Verify(req.CurrentPassword, user.PasswordHash))
            return BadRequest(new { message = "Current password is incorrect." });

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.NewPassword);
        await db.SaveChangesAsync();
        return Ok(new { message = "Password changed successfully." });
    }
}