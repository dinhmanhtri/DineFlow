using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DineFlow.API.Middleware;

/// <summary>
/// Ghi kết quả Health Check ra JSON response thay vì plain text mặc định
/// 
/// [KIẾN THỨC] Health Check Response Format:
/// Mặc định ASP.NET Core chỉ trả về "Healthy"/"Unhealthy" text.
/// Custom ResponseWriter → trả JSON với detail từng dependency:
/// {
///   "status": "Healthy",
///   "duration": "00:00:00.123",
///   "entries": {
///     "sqlserver": { "status": "Healthy", "duration": "00:00:00.050" },
///     "redis":     { "status": "Healthy", "duration": "00:00:00.003" }
///   }
/// }
/// </summary>
public static class HealthCheckResponseWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented        = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static async Task WriteJson(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        // HTTP Status: 200 = Healthy/Degraded, 503 = Unhealthy
        context.Response.StatusCode = report.Status == HealthStatus.Unhealthy ? 503 : 200;

        var response = new
        {
            status   = report.Status.ToString(),
            duration = report.TotalDuration.ToString(@"mm\:ss\.fff"),
            entries  = report.Entries.ToDictionary(
                kvp => kvp.Key,
                kvp => new
                {
                    status      = kvp.Value.Status.ToString(),
                    duration    = kvp.Value.Duration.ToString(@"mm\:ss\.fff"),
                    description = kvp.Value.Description,
                    // Chỉ hiển thị exception message trong Development
                    error       = kvp.Value.Exception?.Message
                })
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response, JsonOptions));
    }
}
