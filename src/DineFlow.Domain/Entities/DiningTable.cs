using DineFlow.Domain.Common;
using DineFlow.Domain.Enums;

namespace DineFlow.Domain.Entities;

/// <summary>
/// Bàn ăn trong nhà hàng
/// 
/// [KIẾN THỨC] Domain Logic trong Entity:
/// Thay vì để logic "bàn có trống không" ở Controller hay Service,
/// ta đặt method IsAvailableForOrder() ngay trong entity.
/// → Đây là "Rich Domain Model" (DDD concept)
/// → Ngược lại là "Anemic Domain Model": entity chỉ là data bag, không có behavior
/// </summary>
public class DiningTable : BaseEntity
{
    public int TableNumber { get; set; }

    /// <summary>Tầng trong nhà hàng (1, 2, 3...)</summary>
    public int FloorNumber { get; set; }

    /// <summary>Số người tối đa</summary>
    public int Capacity { get; set; }

    public TableStatus Status { get; set; } = TableStatus.Available;

    // Navigation: 1 bàn có nhiều orders qua các lần phục vụ
    public ICollection<Order> Orders { get; set; } = new List<Order>();

    // ===== Domain Logic =====

    /// <summary>
    /// Business rule: bàn chỉ có thể đặt order khi đang Available
    /// Logic này thuộc về Domain, không phải Application/Service
    /// </summary>
    public bool IsAvailableForOrder() => Status == TableStatus.Available;

    public void MarkAsOccupied()
    {
        if (Status != TableStatus.Available)
            throw new InvalidOperationException($"Bàn {TableNumber} không thể đặt, trạng thái hiện tại: {Status}");

        Status = TableStatus.Occupied;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsAvailable()
    {
        Status = TableStatus.Available;
        UpdatedAt = DateTime.UtcNow;
    }
}
