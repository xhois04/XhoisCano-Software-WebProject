using System.ComponentModel.DataAnnotations;

namespace AphexXhois.Api.DTOs;

public record RegisterRequest(
    [Required, MinLength(3)] string Username,
    [Required, EmailAddress] string Email,
    [Required, MinLength(6)] string Password
);

public record LoginRequest(
    [Required] string Username,
    [Required] string Password
);

public record AuthResponse(
    string Token,
    int UserId,
    string Username,
    string Role
);

public record ForgotPasswordRequest([Required, EmailAddress] string Email);

public record ResetPasswordRequest(
    [Required] string Token,
    [Required, MinLength(6)] string NewPassword
);