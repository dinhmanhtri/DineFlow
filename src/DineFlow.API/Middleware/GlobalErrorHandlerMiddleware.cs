using System.Net;
using System.Text.Json;
using DineFlow.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace DineFlow.API.Middleware;

/// <summary>
/// Global Error Handler Middleware — bắt tất cả unhandled exceptions
/// 
/// [KIẾN THỨC] Middleware Pipeline trong ASP.NET Core:
/// Request → Middleware 1 → Middleware 2 → ... → Controller → Response
///         ← Middleware 1 ← Middleware 2 ← ... ← Controller ←
/// 
/// GlobalErrorHandler đặt ĐẦU TIÊN trong pipeline:
/// → Bao bọc tất cả middlewares phía sau
/// → Bắt exception từ bất kỳ đâu trong pipeline
/// → Trả về JSON response thay vì HTML error page mặc định
/// 
/// Map Domain Exceptions → HTTP Status Codes:
///   NotFoundException     → 404 Not Found
///   ValidationException   → 400 Bad Request (kèm field errors)
///   UnauthorizedException → 401 Unauthorized
///   ForbiddenException    → 403 Forbidden
///   ConflictException     → 409 Conflict
///   Exception             → 500 Internal Server Error
/// </summary>
public class GlobalErrorHandlerMiddleware(
    RequestDelegate next,
    ILogger<GlobalErrorHandlerMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, response) = exception switch
        {
            NotFoundException ex => (
                HttpStatusCode.NotFound,
                new ApiErrorResponse(404, ex.Message)
            ),

            ValidationException ex => (
                HttpStatusCode.BadRequest,
                new ApiErrorResponse(400, "Dữ liệu không hợp lệ.", ex.Errors)
            ),

            UnauthorizedException ex => (
                HttpStatusCode.Unauthorized,
                new ApiErrorResponse(401, ex.Message)
            ),

            ForbiddenException ex => (
                HttpStatusCode.Forbidden,
                new ApiErrorResponse(403, ex.Message)
            ),

            ConflictException ex => (
                HttpStatusCode.Conflict,
                new ApiErrorResponse(409, ex.Message)
            ),

            NotImplementedException => (
                HttpStatusCode.NotImplemented,
                new ApiErrorResponse(501, "Tính năng chưa được triển khai.")
            ),

            _ => (
                HttpStatusCode.InternalServerError,
                new ApiErrorResponse(500, "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau.")
                // NOTE: Không expose exception message trong production → bảo mật
            )
        };

        context.Response.StatusCode = (int)statusCode;

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}

/// <summary>Standard error response format trả về client</summary>
public record ApiErrorResponse(
    int StatusCode,
    string Message,
    IReadOnlyDictionary<string, string[]>? Errors = null
);

/// <summary>Extension method để đăng ký middleware gọn hơn</summary>
public static class GlobalErrorHandlerMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalErrorHandler(this IApplicationBuilder app)
        => app.UseMiddleware<GlobalErrorHandlerMiddleware>();
}
