using System.Net.Http.Headers;
using System.Text.Json;
using DineFlow.Web.Models;

namespace DineFlow.Web.Services;

public interface IStaffApiClient
{
    Task<List<StaffViewDto>> GetAllStaffAsync();
}

public class StaffApiClient(
    HttpClient           httpClient,
    IHttpContextAccessor httpContextAccessor) : IStaffApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private void AttachToken()
    {
        var token = httpContextAccessor.HttpContext?.Session.GetString("jwt_token");
        httpClient.DefaultRequestHeaders.Authorization = !string.IsNullOrEmpty(token)
            ? new AuthenticationHeaderValue("Bearer", token)
            : null;
    }

    public async Task<List<StaffViewDto>> GetAllStaffAsync()
    {
        AttachToken();
        var response = await httpClient.GetAsync("api/auth/staff");
        if (!response.IsSuccessStatusCode) return [];

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<StaffViewDto>>(json, JsonOptions) ?? [];
    }
}
