using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using DineFlow.Web.Models;

namespace DineFlow.Web.Services;

/// <summary>
/// Auth API Client — gọi /api/auth/* endpoints
/// 
/// [KIẾN THỨC] Typed HttpClient:
/// - Được inject qua DI với HttpClient đã được cấu hình (BaseAddress, Headers)
/// - IHttpClientFactory quản lý lifetime → tránh socket exhaustion
/// </summary>
public interface IAuthApiClient
{
    Task<LoginResponse?> LoginAsync(string email, string password);
}

public record LoginResponse(string Token, string Email, string FullName, string Role, DateTime ExpiresAt);

public class AuthApiClient(HttpClient httpClient) : IAuthApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<LoginResponse?> LoginAsync(string email, string password)
    {
        var payload = new { email, password };
        var content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");

        var response = await httpClient.PostAsync("api/auth/login", content);

        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<LoginResponse>(json, JsonOptions);
    }
}
