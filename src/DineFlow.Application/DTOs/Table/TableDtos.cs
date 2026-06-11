using System.ComponentModel.DataAnnotations;
using DineFlow.Domain.Enums;

namespace DineFlow.Application.DTOs.Table;

public record DiningTableDto(
    Guid Id,
    int TableNumber,
    int FloorNumber,
    int Capacity,
    TableStatus Status,
    string StatusDisplay  // "Trống", "Đang có khách"... dùng cho UI
);

public record CreateTableRequest(
    [Range(1, 999)] int TableNumber,
    [Range(1, 10)] int FloorNumber,
    [Range(2, 20)] int Capacity
);

public record UpdateTableStatusRequest(
    [Required] TableStatus Status
);
