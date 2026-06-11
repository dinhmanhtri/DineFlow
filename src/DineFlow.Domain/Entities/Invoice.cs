using DineFlow.Domain.Common;
using DineFlow.Domain.Enums;

namespace DineFlow.Domain.Entities;

/// <summary>
/// Hóa đơn thanh toán — được tạo từ Order
/// 
/// [KIẾN THỨC] Quan hệ 1-1 (One-to-One):
/// 1 Order có tối đa 1 Invoice (quan hệ 1-1)
/// EF Core cấu hình: Invoice có FK là OrderId (Unique)
/// 
/// Tại sao tách Invoice ra khỏi Order?
/// - SRP (Single Responsibility): Order quản lý việc gọi món, Invoice quản lý thanh toán
/// - Order có thể tồn tại lâu (đang ăn), Invoice chỉ tạo khi checkout
/// - Dễ mở rộng: sau này Invoice có thể có nhiều payment installments
/// </summary>
public class Invoice : BaseEntity
{
    // Foreign Key (1-1 với Order)
    public Guid OrderId { get; set; }

    /// <summary>Tổng trước khi giảm giá và thuế</summary>
    public decimal SubTotal { get; set; }

    /// <summary>Số tiền giảm giá (không phải %)</summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>Thuế VAT — thường 10% ở Việt Nam</summary>
    public decimal TaxAmount { get; set; }

    /// <summary>Tổng cuối = SubTotal - Discount + Tax</summary>
    public decimal Total { get; set; }

    public PaymentMethod PaymentMethod { get; set; }

    /// <summary>Thời điểm thanh toán thực tế</summary>
    public DateTime PaidAt { get; set; }

    /// <summary>Thu ngân xử lý thanh toán</summary>
    public Guid CashierId { get; set; }

    // Navigation Properties
    public Order Order { get; set; } = null!;
    public Staff Cashier { get; set; } = null!;

    // ===== Domain Logic =====

    /// <summary>
    /// Factory method — tạo Invoice từ Order với business rules
    /// Thay vì để Service tính toán ngoài, logic tính giá nằm trong Domain
    /// </summary>
    public static Invoice CreateFromOrder(Order order, decimal discountAmount, Guid cashierId)
    {
        var subTotal = order.TotalAmount;
        var tax = Math.Round((subTotal - discountAmount) * 0.10m, 2);  // VAT 10%
        var total = subTotal - discountAmount + tax;

        return new Invoice
        {
            OrderId        = order.Id,
            SubTotal       = subTotal,
            DiscountAmount = discountAmount,
            TaxAmount      = tax,
            Total          = total,
            PaidAt         = DateTime.UtcNow,
            CashierId      = cashierId
        };
    }
}
