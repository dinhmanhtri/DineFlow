using System.Text.Json;
using DineFlow.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace DineFlow.Infrastructure.Services;

/// <summary>
/// Redis Cache Service Implementation
/// 
/// [KIẾN THỨC] Redis là gì?
/// In-memory data store — lưu data trong RAM → cực nhanh (sub-millisecond)
/// Dùng cho: caching, session, rate limiting, pub/sub, distributed lock
/// 
/// [KIẾN THỨC] IConnectionMultiplexer:
/// - Singleton — connection pool được reuse qua toàn app
/// - KHÔNG tạo new ConnectionMultiplexer mỗi request (expensive!)
/// - IDatabase: lấy 1 logical database từ Redis (default db 0)
/// 
/// [KIẾN THỨC] Graceful Degradation:
/// Redis có thể down → app vẫn phải chạy được (chỉ chậm hơn do cache miss)
/// → try/catch mọi Redis operation → log warning → return null (cache miss)
/// → Service layer sẽ fallback về DB query
/// 
/// [KIẾN THỨC] Cache-Aside Pattern (implemented here):
/// 1. Read: check cache → hit: return từ cache, miss: query DB → write cache → return
/// 2. Write: update DB → invalidate (xóa) cache
/// → Cache luôn là "lazy" copy của DB, không bao giờ là source of truth
/// </summary>
public class RedisCacheService(
    IConnectionMultiplexer redis,
    ILogger<RedisCacheService> logger) : ICacheService
{
    private readonly IDatabase _db     = redis.GetDatabase();
    private readonly IServer   _server = redis.GetServer(redis.GetEndPoints().First());

    private static readonly TimeSpan DefaultExpiry = TimeSpan.FromMinutes(10);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented        = false
    };

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        try
        {
            var value = await _db.StringGetAsync(key);
            if (value.IsNullOrEmpty) return null; // Cache miss

            var json = (string?)value;
            if (json is null) return null;

            return JsonSerializer.Deserialize<T>(json, JsonOptions);
        }
        catch (RedisException ex)
        {
            // [KIẾN THỨC] Graceful degradation: Redis down → không crash app
            logger.LogWarning(ex, "Redis GET failed for key '{Key}'. Falling back to source.", key);
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class
    {
        try
        {
            var json = JsonSerializer.Serialize(value, JsonOptions);
            await _db.StringSetAsync(key, json, expiry ?? DefaultExpiry);
        }
        catch (RedisException ex)
        {
            logger.LogWarning(ex, "Redis SET failed for key '{Key}'. Cache write skipped.", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await _db.KeyDeleteAsync(key);
        }
        catch (RedisException ex)
        {
            logger.LogWarning(ex, "Redis DEL failed for key '{Key}'.", key);
        }
    }

    /// <summary>
    /// Xóa cache theo pattern — dùng SCAN thay vì KEYS để tránh block Redis
    /// 
    /// [KIẾN THỨC] KEYS vs SCAN:
    /// - KEYS menu:*  → block Redis cho đến khi xong (nguy hiểm với 1M+ keys!)
    /// - SCAN cursor COUNT 100 → iterate từng batch, không block
    /// → Luôn dùng SCAN trong production
    /// </summary>
    public async Task RemoveByPatternAsync(string pattern)
    {
        try
        {
            var keys = _server.KeysAsync(pattern: pattern);
            await foreach (var key in keys)
                await _db.KeyDeleteAsync(key);
        }
        catch (RedisException ex)
        {
            logger.LogWarning(ex, "Redis SCAN/DEL failed for pattern '{Pattern}'.", pattern);
        }
    }

    /// <summary>
    /// GetOrSetAsync — Cache-Aside trong 1 method
    /// 
    /// [KIẾN THỨC] Func&lt;Task&lt;T&gt;&gt; pattern:
    /// factory chỉ được gọi khi cache miss → không query DB khi cache hit
    /// Tương đương: if (cached != null) return cached; return await factory();
    /// </summary>
    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null)
        where T : class
    {
        // 1. Try cache first
        var cached = await GetAsync<T>(key);
        if (cached is not null) return cached;

        // 2. Cache miss → call factory (DB query, external API, etc.)
        var result = await factory();

        // 3. Write to cache (fire-and-forget nếu muốn, nhưng await để đảm bảo consistency)
        await SetAsync(key, result, expiry);

        return result;
    }

    /// <summary>Ping Redis — dùng cho Health Checks</summary>
    public async Task<bool> PingAsync()
    {
        try
        {
            var latency = await _db.PingAsync();
            logger.LogDebug("Redis PING: {Latency}ms", latency.TotalMilliseconds);
            return true;
        }
        catch (RedisException ex)
        {
            logger.LogError(ex, "Redis PING failed.");
            return false;
        }
    }
}
