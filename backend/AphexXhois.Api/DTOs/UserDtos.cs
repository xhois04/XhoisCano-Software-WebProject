namespace AphexXhois.Api.DTOs;

public record UserProfileResponse(
    int Id,
    string Username,
    string Email,
    string Role,
    bool IsBanned,
    DateTime CreatedAt
);

public record UserSearchResponse(
    int Id,
    string Username,
    bool IsFriend
);

public record AdminUserResponse(
    int Id,
    string Username,
    string Email,
    string Role,
    bool IsBanned,
    DateTime CreatedAt
);

public record UpdateProfileRequest(string? Username, string? Email);

public record ChangePasswordRequest(
    [System.ComponentModel.DataAnnotations.Required] string CurrentPassword,
    [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.MinLength(6)] string NewPassword
);