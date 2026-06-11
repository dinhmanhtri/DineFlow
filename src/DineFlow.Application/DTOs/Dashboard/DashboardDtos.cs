using DineFlow.Domain.Enums;

namespace DineFlow.Application.DTOs.Dashboard;

/// <summary>
/// Dashboard KPI Summary — dữ liệu tổng quan cho trang chủ
/// </summary>
public record DashboardSummaryDto(
    decimal TodayRevenue,
    int TodayOrders,
    int ActiveTables,        // Số bàn đang có khách
    int AvailableTables,     // Số bàn trống
    decimal RevenueGrowth,   // % tăng trưởng so với hôm qua
    List<TopMenuItemDto> TopItems,
    List<RevenueByHourDto> HourlyRevenue
);

public record TopMenuItemDto(
    string Name,
    int TotalQuantity,
    decimal TotalRevenue
);

public record RevenueByHourDto(
    int Hour,       // 0-23
    decimal Amount
);

public record RevenueReportDto(
    DateTime From,
    DateTime To,
    decimal TotalRevenue,
    int TotalOrders,
    decimal AverageOrderValue,
    List<DailyRevenueDto> DailyBreakdown
);

public record DailyRevenueDto(
    DateTime Date,
    decimal Revenue,
    int OrderCount
);
