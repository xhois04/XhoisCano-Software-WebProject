using System.Security.Claims;
using AphexXhois.Api.Data;
using AphexXhois.Api.DTOs;
using AphexXhois.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AphexXhois.Api.Controllers;

[ApiController]
[Route("api/ratings")]
[Authorize]
public class RatingsController(AppDbContext db) : ControllerBase
{
    private int MyId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    public async Task<IActionResult> Create(CreateRatingRequest req)
    {
        if (await db.SongRatings.AnyAsync(r => r.UserId == MyId && r.SpotifyTrackId == req.SpotifyTrackId))
            return Conflict(new { message = "You already rated this track. Use PUT to update." });

        var rating = new SongRating
        {
            UserId = MyId,
            SpotifyTrackId = req.SpotifyTrackId,
            TrackName = req.TrackName,
            ArtistName = req.ArtistName,
            Rating = req.Rating,
            Review = req.Review
        };

        db.SongRatings.Add(rating);
        await db.SaveChangesAsync();

        var user = await db.Users.FindAsync(MyId);
        return CreatedAtAction(nameof(GetMy), new RatingResponse(
            rating.Id, rating.SpotifyTrackId, rating.TrackName, rating.ArtistName,
            rating.Rating, rating.Review, user!.Username, rating.CreatedAt));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateRatingRequest req)
    {
        var rating = await db.SongRatings.FirstOrDefaultAsync(r => r.Id == id && r.UserId == MyId);
        if (rating == null) return NotFound();

        rating.Rating = req.Rating;
        rating.Review = req.Review;
        await db.SaveChangesAsync();

        var user = await db.Users.FindAsync(MyId);
        return Ok(new RatingResponse(
            rating.Id, rating.SpotifyTrackId, rating.TrackName, rating.ArtistName,
            rating.Rating, rating.Review, user!.Username, rating.CreatedAt));
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMy()
    {
        var ratings = await db.SongRatings
            .Where(r => r.UserId == MyId)
            .Include(r => r.User)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new RatingResponse(
                r.Id, r.SpotifyTrackId, r.TrackName, r.ArtistName,
                r.Rating, r.Review, r.User.Username, r.CreatedAt))
            .ToListAsync();

        return Ok(ratings);
    }

    [HttpGet("friends")]
    public async Task<IActionResult> GetFriendRatings()
    {
        var friendIds = await db.Friends
            .Where(f => f.UserId == MyId)
            .Select(f => f.FriendId)
            .ToListAsync();

        var ratings = await db.SongRatings
            .Where(r => friendIds.Contains(r.UserId))
            .Include(r => r.User)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new RatingResponse(
                r.Id, r.SpotifyTrackId, r.TrackName, r.ArtistName,
                r.Rating, r.Review, r.User.Username, r.CreatedAt))
            .ToListAsync();

        return Ok(ratings);
    }

    [AllowAnonymous]
    [HttpGet("top")]
    public async Task<IActionResult> GetTop()
    {
        var top = await db.SongRatings
            .GroupBy(r => new { r.SpotifyTrackId, r.TrackName, r.ArtistName })
            .Select(g => new
            {
                spotifyTrackId = g.Key.SpotifyTrackId,
                trackName = g.Key.TrackName,
                artistName = g.Key.ArtistName,
                ratingCount = g.Count(),
                avgRating = g.Average(r => (double)r.Rating)
            })
            .OrderByDescending(x => x.ratingCount)
            .ThenByDescending(x => x.avgRating)
            .Take(10)
            .ToListAsync();

        return Ok(top);
    }

    [HttpGet("user/{userId:int}")]
    public async Task<IActionResult> GetByUser(int userId)
    {
        var myId = MyId;
        if (userId != myId)
        {
            var isFriend = await db.Friends.AnyAsync(f => f.UserId == myId && f.FriendId == userId);
            if (!isFriend) return Forbid();
        }

        var ratings = await db.SongRatings
            .Where(r => r.UserId == userId)
            .Include(r => r.User)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new RatingResponse(
                r.Id, r.SpotifyTrackId, r.TrackName, r.ArtistName,
                r.Rating, r.Review, r.User.Username, r.CreatedAt))
            .ToListAsync();

        return Ok(ratings);
    }
}