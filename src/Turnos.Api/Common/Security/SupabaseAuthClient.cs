using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Turnos.Api.Common.Contracts;

namespace Turnos.Api.Common.Security;

public sealed class SupabaseAuthClient : ISupabaseAuthClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SupabaseAuthClient> _logger;

    private readonly string _anonKey;

    public SupabaseAuthClient(HttpClient httpClient, IOptions<SupabaseAuthSettings> options, ILogger<SupabaseAuthClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _httpClient.BaseAddress = new Uri(options.Value.Url.TrimEnd('/') + "/");
        _anonKey = options.Value.AnonKey;
    }

    public async Task<SupabaseUserInfo?> GetUserInfoAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "auth/v1/user");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Headers.Add("apikey", _anonKey);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Supabase auth user endpoint returned {StatusCode}: {Body}", response.StatusCode, errorBody);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var supabaseUser = JsonSerializer.Deserialize<SupabaseUserResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                NumberHandling = JsonNumberHandling.AllowReadingFromString
            });

            if (supabaseUser?.Id is null)
            {
                return null;
            }

            var metadata = supabaseUser.UserMetadata;
            var fullName = metadata?.FullName ?? metadata?.Name;
            var (firstName, lastName) = SplitName(fullName);

            return new SupabaseUserInfo(
                supabaseUser.Id,
                supabaseUser.Email,
                supabaseUser.EmailConfirmedAt is not null || (metadata?.EmailVerified ?? false),
                firstName,
                lastName,
                metadata?.AvatarUrl ?? metadata?.Picture);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating Supabase access token");
            return null;
        }
    }

    private static (string? FirstName, string? LastName) SplitName(string? fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            return (null, null);
        }

        var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1)
        {
            return (parts[0], null);
        }

        return (parts[0], string.Join(' ', parts[1..]));
    }

    private sealed class SupabaseUserResponse
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("email_confirmed_at")]
        public DateTime? EmailConfirmedAt { get; set; }

        [JsonPropertyName("user_metadata")]
        public SupabaseUserMetadata? UserMetadata { get; set; }
    }

    private sealed class SupabaseUserMetadata
    {
        [JsonPropertyName("full_name")]
        public string? FullName { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("avatar_url")]
        public string? AvatarUrl { get; set; }

        [JsonPropertyName("picture")]
        public string? Picture { get; set; }

        [JsonPropertyName("email_verified")]
        public bool EmailVerified { get; set; }
    }
}
