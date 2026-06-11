using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using DineFlow.Web.Models;

namespace DineFlow.Web.Services;

public interface IInvoiceApiClient
{
    Task<List<InvoiceViewDto>> GetTodayInvoicesAsync();
    Task<List<InvoiceViewDto>> GetInvoicesByDateAsync(DateTime date);
    Task<InvoiceViewDto?>      GetInvoiceByOrderAsync(Guid orderId);
    Task<InvoiceViewDto?>      CreateInvoiceAsync(Guid orderId, decimal discountAmount, int paymentMethod);
}

public class InvoiceApiClient(
    HttpClient           httpClient,
    IHttpContextAccessor httpContextAccessor) : IInvoiceApiClient
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

    public Task<List<InvoiceViewDto>> GetTodayInvoicesAsync() =>
        GetInvoicesByDateAsync(DateTime.Today);

    public async Task<List<InvoiceViewDto>> GetInvoicesByDateAsync(DateTime date)
    {
        AttachToken();
        var url      = $"api/invoices/by-date?date={date:yyyy-MM-dd}";
        var response = await httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode) return [];

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<InvoiceViewDto>>(json, JsonOptions) ?? [];
    }

    public async Task<InvoiceViewDto?> GetInvoiceByOrderAsync(Guid orderId)
    {
        AttachToken();
        var response = await httpClient.GetAsync($"api/invoices/by-order/{orderId}");
        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<InvoiceViewDto>(json, JsonOptions);
    }

    public async Task<InvoiceViewDto?> CreateInvoiceAsync(Guid orderId, decimal discountAmount, int paymentMethod)
    {
        AttachToken();
        var payload = new
        {
            OrderId        = orderId,
            DiscountAmount = discountAmount,
            PaymentMethod  = paymentMethod
        };
        var response = await httpClient.PostAsync("api/invoices", ToJson(payload));
        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<InvoiceViewDto>(json, JsonOptions);
    }
}
