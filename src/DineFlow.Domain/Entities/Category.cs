using DineFlow.Domain.Common;

namespace DineFlow.Domain.Entities;

/// <summary>
/// Danh mục món ăn (Khai vị, Món chính, Tráng miệng, Đồ uống...)
/// 
/// [KIẾN THỨC] Navigation Properties:
/// ICollection<MenuItem> MenuItems là Navigation Property — EF Core dùng nó để load
/// các MenuItem liên quan theo quan hệ 1-N (1 Category có nhiều MenuItem)
/// 
/// Lưu ý: không khởi tạo = new List<>() trong constructor
/// vì EF Core có thể inject Lazy Loading proxy vào đây
/// </summary>
public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Thứ tự hiển thị trên menu — số nhỏ hơn hiển thị trước
    /// </summary>
    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation property: 1 Category → N MenuItems
    public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
}
