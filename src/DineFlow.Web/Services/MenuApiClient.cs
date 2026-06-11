using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using DineFlow.Web.Models;

namespace DineFlow.Web.Services;

/// <summary>
/// Menu API Client — gọi /api/menu/* endpoints
/// 
/// [KIẾN THỨC] JWT trong HttpClient:
/// - JWT được lưu trong Session (server-side)
/// - Mỗi request: đọc token từ Session → set Authorization header
/// - Dùng IHttpContextAccessor để access Session từ trong service
/// </summary>
public interface IMenuApiClient
{
    // Categories
    Task<List<CategoryDto>> GetCategoriesAsync();

    // Menu Items
    Task<List<MenuItemDto>>        GetAllItemsAsync();
    Task<MenuItemDto?>             GetItemByIdAsync(Guid id);
    Task<MenuItemDto?>             CreateItemAsync(MenuItemFormViewModel form);
    Task<MenuItemDto?>             UpdateItemAsync(Guid id, MenuItemFormViewModel form);
    Task<bool>                     DeleteItemAsync(Guid id);
    Task<bool>                     ToggleAvailabilityAsync(Guid id);
}

public class MenuApiClient(
    HttpClient              httpClient,
    IHttpContextAccessor    httpContextAccessor) : IMenuApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    // ===== Helper: attach JWT token từ Session vào request header =====
    private void AttachToken()
    {
        var token = httpContextAccessor.HttpContext?.Session.GetString("jwt_token");
        if (!string.IsNullOrEmpty(token))
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        else
            httpClient.DefaultRequestHeaders.Authorization = null;
    }

    private StringContent ToJson<T>(T obj) =>
        new(JsonSerializer.Serialize(obj, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }), Encoding.UTF8, "application/json");

    // ===== Categories =====

    public async Task<List<CategoryDto>> GetCategoriesAsync()
    {
        AttachToken();
        var response = await httpClient.GetAsync("api/menu/categories");
        if (!response.IsSuccessStatusCode) return [];

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<CategoryDto>>(json, JsonOptions) ?? [];
    }

    // ===== Menu Items =====

    public async Task<List<MenuItemDto>> GetAllItemsAsync()
    {
        AttachToken();
        var response = await httpClient.GetAsync("api/menu/items");
        if (!response.IsSuccessStatusCode) return [];

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<MenuItemDto>>(json, JsonOptions) ?? [];
    }

    public async Task<MenuItemDto?> GetItemByIdAsync(Guid id)
    {
        AttachToken();
        var response = await httpClient.GetAsync($"api/menu/items/{id}");
        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<MenuItemDto>(json, JsonOptions);
    }

    public async Task<MenuItemDto?> CreateItemAsync(MenuItemFormViewModel form)
    {
        AttachToken();
        var payload = new
        {
            form.Name,
            form.Description,
            form.Price,
            form.CategoryId,
            form.ImageUrl,
            form.IsAvailable
        };

        var response = await httpClient.PostAsync("api/menu/items", ToJson(payload));
        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<MenuItemDto>(json, JsonOptions);
    }

    public async Task<MenuItemDto?> UpdateItemAsync(Guid id, MenuItemFormViewModel form)
    {
        AttachToken();
        var payload = new
        {
            form.Name,
            form.Description,
            form.Price,
            form.CategoryId,
            form.ImageUrl,
            form.IsAvailable
        };

        var response = await httpClient.PutAsync($"api/menu/items/{id}", ToJson(payload));
        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<MenuItemDto>(json, JsonOptions);
    }

    public async Task<bool> DeleteItemAsync(Guid id)
    {
        AttachToken();
        var response = await httpClient.DeleteAsync($"api/menu/items/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ToggleAvailabilityAsync(Guid id)
    {
        AttachToken();
        var response = await httpClient.PatchAsync($"api/menu/items/{id}/toggle-availability", null);
        return response.IsSuccessStatusCode;
    }
}
