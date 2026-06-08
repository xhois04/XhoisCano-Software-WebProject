using System.ComponentModel.DataAnnotations;

namespace AphexXhois.Api.DTOs;

public record CreateRatingRequest(
    [Required] string SpotifyTrackId,
    [Required] string TrackName,
    [Required] string ArtistName,
    [Range(1, 5)] int Rating,
    string? Review
);

public record UpdateRatingRequest(
    [Range(1, 5)] int Rating,
    string? Review
);

public record RatingResponse(
    int Id,
    string SpotifyTrackId,
    string TrackName,
    string ArtistName,
    int Rating,
    string? Review,
    string Username,
    DateTime CreatedAt
);