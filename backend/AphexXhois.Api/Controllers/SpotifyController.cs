using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AphexXhois.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AphexXhois.Api.Controllers;

[ApiController]
[Route("api/spotify")]
public class SpotifyController(SpotifyService spotify, IConfiguration config, IHttpClientFactory httpClientFactory) : ControllerBase
{
    [HttpGet("search")]
    [Authorize]
    public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] int limit = 20)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest(new { message = "Query required." });

        try
        {
            var results = await spotify.SearchTracksAsync(q, Math.Clamp(limit, 1, 50));
            return Ok(results);
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(502, new { message = "Spotify API error.", detail = ex.Message });
        }
    }

    [HttpGet("auth/url")]
    [Authorize]
    public IActionResult GetAuthUrl()
    {
        var clientId    = config["Spotify:ClientId"];
        var redirectUri = config["Spotify:RedirectUri"];
        var scopes = "streaming user-read-email user-read-private user-read-playback-state user-modify-playback-state";
        var url = $"https://accounts.spotify.com/authorize" +
                  $"?client_id={clientId}" +
                  $"&response_type=code" +
                  $"&redirect_uri={Uri.EscapeDataString(redirectUri!)}" +
                  $"&scope={Uri.EscapeDataString(scopes)}";
        return Ok(new { url, clientId, redirectUri });
    }

    [HttpGet("callback")]
    public async Task<IActionResult> Callback([FromQuery] string? code, [FromQuery] string? error)
    {
        if (error != null || code == null)
            return Redirect($"http://localhost:5222/#spotify_error={error ?? "unknown"}");

        var clientId     = config["Spotify:ClientId"];
        var clientSecret = config["Spotify:ClientSecret"];
        var redirectUri  = config["Spotify:RedirectUri"];

        var client = httpClientFactory.CreateClient();
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
        var request = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token");
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"]   = "authorization_code",
            ["code"]         = code,
            ["redirect_uri"] = redirectUri!
        });

        var response = await client.SendAsync(request);
        if (!response.IsSuccessStatusCode)
            return Redirect("http://localhost:5222/#spotify_error=token_exchange_failed");

        var json          = await response.Content.ReadAsStringAsync();
        var doc           = JsonDocument.Parse(json);
        var accessToken   = doc.RootElement.GetProperty("access_token").GetString();
        var refreshToken  = doc.RootElement.GetProperty("refresh_token").GetString();
        var expiresIn     = doc.RootElement.GetProperty("expires_in").GetInt32();

        return Redirect($"http://localhost:5222/#spotify_token={accessToken}&spotify_refresh={refreshToken}&spotify_expires={expiresIn}");
    }

    [HttpPost("refresh")]
    [Authorize]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest req)
    {
        var clientId     = config["Spotify:ClientId"];
        var clientSecret = config["Spotify:ClientSecret"];

        var client = httpClientFactory.CreateClient();
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
        var request = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token");
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"]    = "refresh_token",
            ["refresh_token"] = req.RefreshToken
        });

        var response = await client.SendAsync(request);
        if (!response.IsSuccessStatusCode)
            return StatusCode(502, new { message = "Failed to refresh token." });

        var json        = await response.Content.ReadAsStringAsync();
        var doc         = JsonDocument.Parse(json);
        var accessToken = doc.RootElement.GetProperty("access_token").GetString();
        var expiresIn   = doc.RootElement.GetProperty("expires_in").GetInt32();

        return Ok(new { accessToken, expiresIn });
    }
}

public record RefreshRequest(string RefreshToken);
