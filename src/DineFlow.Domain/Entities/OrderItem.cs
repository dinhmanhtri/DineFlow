using DineFlow.Domain.Common;

namespace DineFlow.Domain.Entities;

/// <summary>
/// Chi tiết từng món trong một đơn hàng
/// 
/// [KIẾN THỨC] Tại sao lưu UnitPrice thay vì chỉ lưu MenuItemId?
/// 
/// Vấn đề: nếu chỉ lưu MenuItemId và tính giá theo MenuItem.Price hiện tại:
/// → Giá món thay đổi sau khi order → hóa đơn cũ tự động thay đổi → SAI!
/// 
/// Giải pháp: Snapshot Price tại thời điểm đặt vào UnitPrice
/// → Dù admin đổi giá sau, hóa đơn cũ vẫn đúng
/// → Đây là "Point-in-time snapshot" pattern
/// </summary>
public class OrderItem : BaseEntity
{
    // Foreign Keys
    public Guid OrderId    { get; set; }
    public Guid MenuItemId { get; set; }

    /// <summary>
    /// Giá tại thời điểm đặt — SNAPSHOT, không phải MenuItem.Price hiện tại
    /// </summary>
    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    public string? Note { get; set; }  // Ghi chú riêng cho món này (ít cay, không hành...)

    // Navigation Properties
    public Order Order { get; set; } = null!;
    public MenuItem MenuItem { get; set; } = null!;

    /// <summary>Thành tiền = đơn giá × số lượng</summary>
    public decimal SubTotal => UnitPrice * Quantity;  // Computed property — không cột trong DB
}
