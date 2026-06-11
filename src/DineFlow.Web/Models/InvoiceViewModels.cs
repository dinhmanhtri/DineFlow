using System.ComponentModel.DataAnnotations;

namespace DineFlow.Web.Models;

// ===== DTOs (mirror từ Application layer) =====

public record InvoiceViewDto(
    Guid     Id,
    Guid     OrderId,
    int      TableNumber,
    decimal  SubTotal,
    decimal  DiscountAmount,
    decimal  TaxAmount,
    decimal  Total,
    int      PaymentMethod,         // enum int
    string   PaymentMethodDisplay,
    string   CashierName,
    DateTime PaidAt);

// ===== ViewModels =====

public class InvoiceIndexViewModel
{
    public IEnumerable<InvoiceViewDto> Invoices      { get; set; } = [];
    public decimal                     TodayRevenue  { get; set; }
    public int                         TodayCount    { get; set; }
    public DateTime                    Date          { get; set; } = DateTime.Today;
}

public class CreateInvoiceViewModel
{
    [Required]
    public Guid OrderId { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Giảm giá không được âm")]
    [Display(Name = "Giảm giá (VND)")]
    public decimal DiscountAmount { get; set; } = 0;

    [Required]
    [Display(Name = "Phương thức thanh toán")]
    public int PaymentMethod { get; set; } = 0; // 0=Cash

    // Thông tin hiển thị (read-only)
    public int     TableNumber  { get; set; }
    public decimal SubTotal     { get; set; }
    public decimal TaxAmount    { get; set; }
    public decimal Total        => SubTotal + TaxAmount - DiscountAmount;
}
