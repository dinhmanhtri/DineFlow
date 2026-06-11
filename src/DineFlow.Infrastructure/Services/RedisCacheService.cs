using System.Text.Json;
using DineFlow.Application.Interfaces.Services;
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
/// [KIẾN THỨC] Serialization:
/// Redis lưu string/byte[] → phải serialize object sang JSON trước
/// System.Text.Json: built-in, nhanh hơn Newtonsoft.Json
/// 
/// [KIẾN THỨC] Cache-Aside Pattern (implemented here):
/// 1. Read: check cache → hit: return từ cache, miss: query DB → write cache → return
/// 2. Write: update DB → invalidate (xóa) cache
/// → Cache luôn là "lazy" copy của DB, không bao giờ là source of truth
/// </summary>
public class RedisCacheService : ICacheService
{
    private readonly IDatabase _db;
    private readonly IServer _server;
    private static readonly TimeSpan DefaultExpiry = TimeSpan.FromMinutes(10);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public RedisCacheService(IConnectionMultiplexer redis)
    {
        _db     = redis.GetDatabase();
        _server = redis.GetServer(redis.GetEndPoints().First());
    }

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        var value = await _db.StringGetAsync(key);

        if (value.IsNullOrEmpty) return null; // Cache miss

        // Cast RedisValue → string? rõ ràng để tránh ambiguous overload
        // (RedisValue implicit convert sang cả string và ReadOnlySpan<byte>)
        var json = (string?)value;
        if (json is null) return null;

        return JsonSerializer.Deserialize<T>(json, JsonOptions);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class
    {
        var json = JsonSerializer.Serialize(value, JsonOptions);

        // StringSetAsync → SET key value EX <seconds>
        await _db.StringSetAsync(key, json, expiry ?? DefaultExpiry);
    }

    public async Task RemoveAsync(string key)
        => await _db.KeyDeleteAsync(key);

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
        var keys = _server.KeysAsync(pattern: pattern);

        await foreach (var key in keys)
        {
            await _db.KeyDeleteAsync(key);
        }
    }
}
