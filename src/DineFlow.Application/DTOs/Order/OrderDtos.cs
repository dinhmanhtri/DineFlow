using System.ComponentModel.DataAnnotations;
using DineFlow.Domain.Enums;

namespace DineFlow.Application.DTOs.Order;

// ===== Output DTOs =====

public record OrderItemDto(
    Guid Id,
    Guid MenuItemId,
    string MenuItemName,
    decimal UnitPrice,
    int Quantity,
    decimal SubTotal,    // UnitPrice * Quantity
    string? Note
);

public record OrderDto(
    Guid Id,
    int TableNumber,
    int FloorNumber,
    string StaffName,
    OrderStatus Status,
    string StatusDisplay,
    decimal TotalAmount,
    string? Note,
    DateTime CreatedAt,
    List<OrderItemDto> Items
);

/// <summary>Summary cho danh sách orders — không load Items (hiệu năng)</summary>
public record OrderSummaryDto(
    Guid Id,
    int TableNumber,
    string StaffName,
    OrderStatus Status,
    string StatusDisplay,
    decimal TotalAmount,
    int ItemCount,
    DateTime CreatedAt
);

// ===== Request DTOs =====

public record CreateOrderRequest(
    [Required(ErrorMessage = "Bàn không được để trống")]
    Guid TableId,

    /// <summary>Items tùy chọn — có thể tạo order rồi thêm món sau</summary>
    List<AddOrderItemRequest>? InitialItems
);

public record AddOrderItemRequest(
    [Required] Guid MenuItemId,
    [Range(1, 99, ErrorMessage = "Số lượng phải từ 1 đến 99")] int Quantity,
    [MaxLength(300)] string? Note
);

public record UpdateOrderStatusRequest(
    [Required] OrderStatus NewStatus
);
