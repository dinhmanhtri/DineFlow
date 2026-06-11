using DineFlow.Domain.Common;

namespace DineFlow.Domain.Entities;

/// <summary>
/// Món ăn trong menu nhà hàng
/// 
/// [KIẾN THỨC] Foreign Key vs Navigation Property:
/// - CategoryId (Guid): Foreign Key — cột thực sự trong DB
/// - Category: Navigation Property — EF Core dùng để JOIN, không có cột trong DB
/// 
/// EF Core tự hiểu: nếu có property "CategoryId" và Navigation "Category"
/// → tự tạo FK relationship mà không cần Fluent API hay DataAnnotation
/// (Convention over Configuration)
/// </summary>
public class MenuItem : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    /// <summary>
    /// Dùng decimal thay vì double/float cho tiền tệ!
    /// double: lưu binary, mất precision → 0.1 + 0.2 ≠ 0.3
    /// decimal: lưu decimal, chính xác → an toàn cho tính toán tài chính
    /// </summary>
    public decimal Price { get; set; }

    public string? ImageUrl { get; set; }

    public bool IsAvailable { get; set; } = true;

    // Foreign Key
    public Guid CategoryId { get; set; }

    // Navigation Property (Many-to-One: nhiều MenuItem về 1 Category)
    public Category Category { get; set; } = null!;

    // null! = null-forgiving operator: "tôi biết đây null nhưng EF Core sẽ inject vào"
    // Tránh compiler warning CS8618 (non-nullable reference type uninitialized)
}
