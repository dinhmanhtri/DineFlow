using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using DineFlow.Web.Models;

namespace DineFlow.Web.Services;

public interface IOrderApiClient
{
    Task<List<OrderSummaryViewDto>> GetAllOrdersAsync(int? status = null);
    Task<OrderViewDto?>             GetOrderByIdAsync(Guid id);
    Task<OrderViewDto?>             CreateOrderAsync(Guid tableId, string? note = null);
    Task<OrderViewDto?>             AddItemToOrderAsync(Guid orderId, Guid menuItemId, int quantity, string? note);
    Task<bool>                      RemoveItemFromOrderAsync(Guid orderId, Guid orderItemId);
    Task<bool>                      UpdateOrderStatusAsync(Guid orderId, int newStatus);
    Task<bool>                      CancelOrderAsync(Guid orderId);
}

public class OrderApiClient(
    HttpClient           httpClient,
    IHttpContextAccessor httpContextAccessor) : IOrderApiClient
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

    public async Task<List<OrderSummaryViewDto>> GetAllOrdersAsync(int? status = null)
    {
        AttachToken();
        var url      = status.HasValue ? $"api/orders?status={status}" : "api/orders";
        var response = await httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode) return [];

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<OrderSummaryViewDto>>(json, JsonOptions) ?? [];
    }

    public async Task<OrderViewDto?> GetOrderByIdAsync(Guid id)
    {
        AttachToken();
        var response = await httpClient.GetAsync($"api/orders/{id}");
        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<OrderViewDto>(json, JsonOptions);
    }

    public async Task<OrderViewDto?> CreateOrderAsync(Guid tableId, string? note = null)
    {
        AttachToken();
        var payload = new { TableId = tableId };
        var response = await httpClient.PostAsync("api/orders", ToJson(payload));
        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<OrderViewDto>(json, JsonOptions);
    }

    public async Task<OrderViewDto?> AddItemToOrderAsync(Guid orderId, Guid menuItemId, int quantity, string? note)
    {
        AttachToken();
        var payload  = new { MenuItemId = menuItemId, Quantity = quantity, Note = note };
        var response = await httpClient.PostAsync($"api/orders/{orderId}/items", ToJson(payload));
        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<OrderViewDto>(json, JsonOptions);
    }

    public async Task<bool> RemoveItemFromOrderAsync(Guid orderId, Guid orderItemId)
    {
        AttachToken();
        var response = await httpClient.DeleteAsync($"api/orders/{orderId}/items/{orderItemId}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateOrderStatusAsync(Guid orderId, int newStatus)
    {
        AttachToken();
        var payload  = new { NewStatus = newStatus };
        var response = await httpClient.PatchAsync($"api/orders/{orderId}/status", ToJson(payload));
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> CancelOrderAsync(Guid orderId)
    {
        AttachToken();
        var response = await httpClient.PostAsync($"api/orders/{orderId}/cancel", null);
        return response.IsSuccessStatusCode;
    }
}
