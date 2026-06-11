using System.Net.Http.Headers;
using System.Text.Json;
using DineFlow.Web.Models;

namespace DineFlow.Web.Services;

public interface IDashboardApiClient
{
    Task<DashboardSummaryViewModel?> GetTodaySummaryAsync();
}

public class DashboardApiClient(
    HttpClient           httpClient,
    IHttpContextAccessor httpContextAccessor) : IDashboardApiClient
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

    public async Task<DashboardSummaryViewModel?> GetTodaySummaryAsync()
    {
        AttachToken();
        var response = await httpClient.GetAsync("api/dashboard/today");
        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();
        // Deserialise trực tiếp vào ViewModel (field names khớp)
        return JsonSerializer.Deserialize<DashboardSummaryViewModel>(json, JsonOptions);
    }
}
