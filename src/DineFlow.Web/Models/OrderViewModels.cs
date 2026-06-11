using System.ComponentModel.DataAnnotations;

namespace DineFlow.Web.Models;

// ===== DTOs (mirror từ Application layer) =====

public record OrderItemViewDto(
    Guid    Id,
    Guid    MenuItemId,
    string  MenuItemName,
    decimal UnitPrice,
    int     Quantity,
    decimal SubTotal,
    string? Note);

public record OrderViewDto(
    Guid                     Id,
    int                      TableNumber,
    int                      FloorNumber,
    string                   StaffName,
    int                      Status,         // enum int
    string                   StatusDisplay,
    decimal                  TotalAmount,
    string?                  Note,
    DateTime                 CreatedAt,
    List<OrderItemViewDto>   Items);

public record OrderSummaryViewDto(
    Guid     Id,
    int      TableNumber,
    string   StaffName,
    int      Status,
    string   StatusDisplay,
    decimal  TotalAmount,
    int      ItemCount,
    DateTime CreatedAt);

// ===== ViewModels =====

public class OrderIndexViewModel
{
    public IEnumerable<OrderSummaryViewDto> Orders      { get; set; } = [];
    public int?                             StatusFilter { get; set; }
    public int  TotalOrders   { get; set; }
    public int  PendingCount  { get; set; }
    public int  PreparingCount { get; set; }
    public int  ServedCount   { get; set; }
}

public class OrderDetailViewModel
{
    public OrderViewDto          Order      { get; set; } = null!;
    public IEnumerable<MenuItemDto> MenuItems { get; set; } = [];
}

public class CreateOrderViewModel
{
    [Required(ErrorMessage = "Vui lòng chọn bàn")]
    [Display(Name = "Bàn")]
    public Guid TableId { get; set; }

    [Display(Name = "Ghi chú")]
    [MaxLength(300)]
    public string? Note { get; set; }

    public IEnumerable<DiningTableDto> AvailableTables { get; set; } = [];
}

public class AddOrderItemViewModel
{
    public Guid OrderId { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn món")]
    [Display(Name = "Món ăn")]
    public Guid MenuItemId { get; set; }

    [Required]
    [Range(1, 99, ErrorMessage = "Số lượng phải từ 1 đến 99")]
    [Display(Name = "Số lượng")]
    public int Quantity { get; set; } = 1;

    [MaxLength(300)]
    [Display(Name = "Ghi chú")]
    public string? Note { get; set; }

    public IEnumerable<MenuItemDto> MenuItems { get; set; } = [];
}
