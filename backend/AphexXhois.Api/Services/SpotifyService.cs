using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AphexXhois.Api.Services;

public class SpotifyService(IConfiguration config, IHttpClientFactory httpClientFactory)
{
    private string? _accessToken;
    private DateTime _tokenExpiry = DateTime.MinValue;

    public async Task<string> GetAccessTokenAsync()
    {
        if (_accessToken != null && DateTime.UtcNow < _tokenExpiry)
            return _accessToken;

        var clientId = config["Spotify:ClientId"]!;
        var clientSecret = config["Spotify:ClientSecret"]!;

        var client = httpClientFactory.CreateClient();
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));

        var request = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token");
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials"
        });

        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        _accessToken = doc.RootElement.GetProperty("access_token").GetString()!;
        var expiresIn = doc.RootElement.GetProperty("expires_in").GetInt32();
        _tokenExpiry = DateTime.UtcNow.AddSeconds(expiresIn - 60);

        return _accessToken;
    }

    public async Task<JsonElement> SearchTracksAsync(string query, int limit = 20)
    {
        var token = await GetAccessTokenAsync();
        var client = httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var encoded = Uri.EscapeDataString(query);
        var url = $"https://api.spotify.com/v1/search?q={encoded}&type=track&limit={limit}";

        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        return JsonDocument.Parse(json).RootElement.Clone();
    }
}