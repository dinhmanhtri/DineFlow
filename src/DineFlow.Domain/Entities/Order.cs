using DineFlow.Domain.Common;
using DineFlow.Domain.Enums;

namespace DineFlow.Domain.Entities;

/// <summary>
/// Đơn hàng — trung tâm của nghiệp vụ nhà hàng
/// 
/// [KIẾN THỨC] Aggregate Root (DDD):
/// Order là Aggregate Root — nó "sở hữu" OrderItems.
/// Rule: chỉ được add/remove OrderItem thông qua Order, không trực tiếp
/// → Đảm bảo business rules: không thể add item vào order đã Paid
/// 
/// Các method AddItem(), RemoveItem(), CalculateTotal() là domain behavior
/// </summary>
public class Order : BaseEntity
{
    // Foreign Keys
    public Guid TableId { get; set; }
    public Guid StaffId { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public string? Note { get; set; }

    /// <summary>Tổng tiền — tính từ OrderItems</summary>
    public decimal TotalAmount { get; set; }

    // Navigation Properties
    public DiningTable Table { get; set; } = null!;
    public Staff Staff { get; set; } = null!;
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public Invoice? Invoice { get; set; }

    // ===== Domain Logic (Aggregate Root behaviors) =====

    /// <summary>
    /// Thêm món vào order — validate business rules trước
    /// </summary>
    public void AddItem(Guid menuItemId, decimal unitPrice, int quantity, string? note = null)
    {
        if (Status != OrderStatus.Pending && Status != OrderStatus.Preparing)
            throw new InvalidOperationException("Không thể thêm món khi order đã hoàn tất hoặc bị hủy.");

        if (quantity <= 0)
            throw new ArgumentException("Số lượng phải lớn hơn 0.");

        // Nếu món đã có trong order → tăng số lượng thay vì thêm mới
        var existing = OrderItems.FirstOrDefault(x => x.MenuItemId == menuItemId);
        if (existing is not null)
        {
            existing.Quantity += quantity;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            OrderItems.Add(new OrderItem
            {
                OrderId    = Id,
                MenuItemId = menuItemId,
                UnitPrice  = unitPrice,
                Quantity   = quantity,
                Note       = note
            });
        }

        RecalculateTotal();
    }

    /// <summary>
    /// Tính lại TotalAmount từ tất cả OrderItems
    /// Encapsulation: bên ngoài không tự set TotalAmount, phải gọi qua method này
    /// </summary>
    public void RecalculateTotal()
    {
        TotalAmount = OrderItems.Sum(x => x.UnitPrice * x.Quantity);
        UpdatedAt = DateTime.UtcNow;
    }

    public bool CanBeCancelled() =>
        Status == OrderStatus.Pending || Status == OrderStatus.Preparing;

    /// <summary>
    /// Xóa món khỏi order — validate business rules
    /// </summary>
    public void RemoveItem(Guid orderItemId)
    {
        if (Status != OrderStatus.Pending && Status != OrderStatus.Preparing)
            throw new InvalidOperationException("Không thể xóa món khi order đã hoàn tất hoặc bị hủy.");

        var item = OrderItems.FirstOrDefault(x => x.Id == orderItemId)
            ?? throw new InvalidOperationException($"OrderItem '{orderItemId}' không thuộc order này.");

        OrderItems.Remove(item);
        RecalculateTotal();
    }

    public void Cancel()
    {
        if (!CanBeCancelled())
            throw new InvalidOperationException($"Không thể hủy order ở trạng thái {Status}.");

        Status = OrderStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }
}
