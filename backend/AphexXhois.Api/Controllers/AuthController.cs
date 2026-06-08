using System.Security.Cryptography;
using AphexXhois.Api.Data;
using AphexXhois.Api.DTOs;
using AphexXhois.Api.Models;
using AphexXhois.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AphexXhois.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(AppDbContext db, JwtService jwt) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest req)
    {
        if (await db.Users.AnyAsync(u => u.Username == req.Username))
            return Conflict(new { message = "Username already taken." });

        if (await db.Users.AnyAsync(u => u.Email == req.Email))
            return Conflict(new { message = "Email already registered." });

        var user = new User
        {
            Username = req.Username,
            Email = req.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password)
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var token = jwt.GenerateToken(user);
        return Ok(new AuthResponse(token, user.Id, user.Username, user.Role.ToString()));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest req)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Username == req.Username);

        if (user == null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return Unauthorized(new { message = "Invalid credentials." });

        if (user.IsBanned)
            return Forbid();

        var token = jwt.GenerateToken(user);
        return Ok(new AuthResponse(token, user.Id, user.Username, user.Role.ToString()));
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest req)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == req.Email);
        if (user == null)
            return Ok(new { message = "If that email exists a reset token has been generated." });

        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        user.ResetToken = token;
        user.ResetTokenExpiry = DateTime.UtcNow.AddHours(1);
        await db.SaveChangesAsync();

        return Ok(new { message = "Reset token generated.", resetToken = token });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest req)
    {
        var user = await db.Users.FirstOrDefaultAsync(
            u => u.ResetToken == req.Token && u.ResetTokenExpiry > DateTime.UtcNow);

        if (user == null)
            return BadRequest(new { message = "Invalid or expired reset token." });

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.NewPassword);
        user.ResetToken = null;
        user.ResetTokenExpiry = null;
        await db.SaveChangesAsync();

        return Ok(new { message = "Password reset successfully. You can now log in." });
    }
}