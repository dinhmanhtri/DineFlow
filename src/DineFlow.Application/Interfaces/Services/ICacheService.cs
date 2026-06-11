namespace DineFlow.Application.Interfaces.Services;

/// <summary>
/// Interface cho Cache Service — tuân thủ DIP (Dependency Inversion Principle)
/// 
/// [KIẾN THỨC] Tại sao define Interface ở Application layer, không phải Infrastructure?
/// 
/// Application layer KHÔNG được biết Redis tồn tại (Infrastructure detail).
/// Application chỉ biết: "có thứ gì đó có thể cache/get/remove" → ICacheService
/// 
/// Infrastructure layer IMPLEMENT ICacheService bằng Redis (hoặc MemoryCache, Memcached...)
/// → Muốn đổi từ Redis sang MemoryCache: chỉ đổi implementation, Application không thay đổi
/// → Đây là Open/Closed Principle: Application "closed" với thay đổi của caching technology
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Lấy giá trị từ cache theo key.
    /// Trả về null nếu không có (cache miss).
    /// </summary>
    Task<T?> GetAsync<T>(string key) where T : class;

    /// <summary>
    /// Lưu giá trị vào cache với TTL (Time To Live).
    /// Sau TTL, cache tự expire.
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class;

    /// <summary>
    /// Xóa cache theo key — dùng khi data thay đổi (cache invalidation)
    /// </summary>
    Task RemoveAsync(string key);

    /// <summary>
    /// Xóa nhiều cache keys theo pattern (dùng wildcard: "menu:*")
    /// </summary>
    Task RemoveByPatternAsync(string pattern);
}
