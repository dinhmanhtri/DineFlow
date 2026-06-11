using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using DineFlow.Web.Models;

namespace DineFlow.Web.Services;

public interface ITableApiClient
{
    Task<List<DiningTableDto>> GetAllTablesAsync(int? floor = null);
    Task<DiningTableDto?>      GetTableByIdAsync(Guid id);
    Task<DiningTableDto?>      CreateTableAsync(TableFormViewModel form);
    Task<bool>                 UpdateTableStatusAsync(Guid id, int status);
    Task<bool>                 DeleteTableAsync(Guid id);
}

public class TableApiClient(
    HttpClient           httpClient,
    IHttpContextAccessor httpContextAccessor) : ITableApiClient
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

    private StringContent ToJson<T>(T obj) =>
        new(JsonSerializer.Serialize(obj, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }), Encoding.UTF8, "application/json");

    public async Task<List<DiningTableDto>> GetAllTablesAsync(int? floor = null)
    {
        AttachToken();
        var url      = floor.HasValue ? $"api/tables?floor={floor}" : "api/tables";
        var response = await httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode) return [];

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<DiningTableDto>>(json, JsonOptions) ?? [];
    }

    public async Task<DiningTableDto?> GetTableByIdAsync(Guid id)
    {
        AttachToken();
        var response = await httpClient.GetAsync($"api/tables/{id}");
        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<DiningTableDto>(json, JsonOptions);
    }

    public async Task<DiningTableDto?> CreateTableAsync(TableFormViewModel form)
    {
        AttachToken();
        var payload = new
        {
            form.TableNumber,
            form.FloorNumber,
            form.Capacity
        };
        var response = await httpClient.PostAsync("api/tables", ToJson(payload));
        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<DiningTableDto>(json, JsonOptions);
    }

    public async Task<bool> UpdateTableStatusAsync(Guid id, int status)
    {
        AttachToken();
        var payload  = new { Status = status };
        var response = await httpClient.PatchAsync($"api/tables/{id}/status", ToJson(payload));
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteTableAsync(Guid id)
    {
        AttachToken();
        var response = await httpClient.DeleteAsync($"api/tables/{id}");
        return response.IsSuccessStatusCode;
    }
}
