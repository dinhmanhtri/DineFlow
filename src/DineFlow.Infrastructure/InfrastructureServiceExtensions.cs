using DineFlow.Application.Interfaces;
using DineFlow.Application.Interfaces.Services;
using DineFlow.Infrastructure.Data;
using DineFlow.Infrastructure.Services;
using DineFlow.Infrastructure.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace DineFlow.Infrastructure;

/// <summary>
/// Extension method để đăng ký tất cả Infrastructure services vào DI container
/// 
/// [KIẾN THỨC] Extension Methods:
/// static class + static method với "this" parameter đầu tiên
/// → Cho phép gọi như method của IServiceCollection:
///   builder.Services.AddInfrastructure(builder.Configuration)
/// → Clean code: Program.cs không biết chi tiết implementation
/// 
/// [KIẾN THỨC] Service Lifetimes:
/// - Singleton:  1 instance cho toàn app lifecycle  → Redis connection
/// - Scoped:     1 instance per HTTP request         → DbContext, UnitOfWork
/// - Transient:  new instance mỗi lần inject         → Stateless services nhỏ
/// 
/// TẠI SAO DbContext phải Scoped?
/// - Singleton → 1 DbContext share giữa requests → race condition!
/// - Transient → mỗi lần inject tạo mới → mất Unit of Work pattern
/// - Scoped    → 1 per request → đúng
/// </summary>
public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ===== Database (SQL Server) =====
        services.AddDbContext<DineFlowDbContext>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("SqlServer"),
                sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,                          // Retry 5 lần
                        maxRetryDelay: TimeSpan.FromSeconds(30),   // Tối đa 30s giữa retries
                        errorNumbersToAdd: null                    // Retry với mọi transient errors
                    );
                    sqlOptions.CommandTimeout(30);  // Query timeout 30 giây
                }
            );

            // Development: log SQL queries chi tiết
            // Production: bỏ để tránh lộ thông tin nhạy cảm
        });

        // ===== Redis (Singleton — connection pool) =====
        var redisConnectionString = configuration.GetConnectionString("Redis")
            ?? "localhost:6379";

        services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(redisConnectionString));

        // ===== Application Services =====
        services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();
        services.AddScoped<ICacheService, RedisCacheService>();

        return services;
    }

    /// <summary>
    /// Tự động chạy migrations và seed data khi app start
    /// Chỉ dùng trong Development/Staging — Production nên chạy migrations riêng
    /// </summary>
    public static async Task InitializeDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DineFlowDbContext>();

        // Áp dụng tất cả pending migrations
        await context.Database.MigrateAsync();

        Console.WriteLine("✅ Database migrated successfully.");
    }
}
