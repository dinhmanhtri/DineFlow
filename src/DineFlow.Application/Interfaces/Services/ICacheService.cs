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

    /// <summary>
    /// Cache-Aside pattern trong 1 dòng code:
    /// Tự động get từ cache → miss → gọi factory → write cache → return
    /// 
    /// [KIẾN THỨC] Func&lt;Task&lt;T&gt;&gt; factory:
    /// Truyền vào 1 async delegate (lambda) sẽ chỉ được gọi khi cache miss.
    /// → Không query DB nếu cache hit (lazy evaluation)
    /// 
    /// Ví dụ dùng:
    ///   var data = await cache.GetOrSetAsync(
    ///       "menu:items:all",
    ///       () => unitOfWork.Menus.GetAllWithCategoryAsync(),
    ///       TimeSpan.FromMinutes(10));
    /// </summary>
    Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null)
        where T : class;

    /// <summary>
    /// Kiểm tra Redis có đang online không (dùng cho Health Check)
    /// </summary>
    Task<bool> PingAsync();
}
