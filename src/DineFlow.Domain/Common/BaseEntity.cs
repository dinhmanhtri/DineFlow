namespace DineFlow.Domain.Common;

/// <summary>
/// Base class cho tất cả Domain Entities.
/// 
/// [KIẾN THỨC] Abstract class vs Interface:
/// - Abstract class: có thể chứa implementation (CreatedAt được gán tự động)
/// - Interface: chỉ định nghĩa contract, không có implementation
/// 
/// Dùng abstract vì: mọi entity đều cần logic giống nhau (auto-generate Id, timestamp)
/// → Template Method Pattern: định nghĩa skeleton, subclass chỉ cần thêm fields riêng
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Dùng Guid thay int vì:
    /// 1. Unique trên toàn hệ thống → safe khi merge data từ nhiều DB
    /// 2. Không lộ thông tin (int Id=5 → hacker biết chỉ có 5 records)
    /// 3. Có thể generate ở client-side trước khi insert
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Nullable vì record mới chưa có UpdatedAt
    /// DateTime? = Nullable<DateTime>
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
