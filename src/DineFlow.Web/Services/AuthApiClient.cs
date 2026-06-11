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

/// [KIẾN THỨC] AccessToken phải khớp với tên field trong JSON response của API:
/// API trả về: { "accessToken": "...", "expiresAt": ..., "fullName": ..., "email": ..., "role": ... }
/// → PropertyNameCaseInsensitive = true sẽ map đúng kể cả Pascal/Camel case
public record LoginResponse(string AccessToken, string Email, string FullName, string Role, DateTime ExpiresAt);

public class AuthApiClient(HttpClient httpClient) : IAuthApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<LoginResponse?> LoginAsync(string email, string password)
    {
        // [KIẾN THỨC] Serialize với CamelCase để API nhận đúng field names
        var payload = new { email, password };
        var json    = JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync("api/auth/login", content);

        if (!response.IsSuccessStatusCode)
            return null;

        var responseJson = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<LoginResponse>(responseJson, JsonOptions);
    }
}
