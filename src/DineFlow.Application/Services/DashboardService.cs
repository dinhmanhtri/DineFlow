using DineFlow.Application.DTOs.Dashboard;
using DineFlow.Application.Interfaces;
using DineFlow.Application.Interfaces.Services;
using DineFlow.Domain.Enums;

namespace DineFlow.Application.Services;

public class DashboardService(
    IUnitOfWork unitOfWork,
    ICacheService cache) : IDashboardService
{
    public async Task<DashboardSummaryDto> GetTodaySummaryAsync()
    {
        var cacheKey = $"dashboard:summary:{DateTime.UtcNow:yyyy-MM-dd}";
        var cached   = await cache.GetAsync<DashboardSummaryDto>(cacheKey);
        if (cached is not null) return cached;

        var today     = DateTime.UtcNow.Date;
        var tomorrow  = today.AddDays(1);

        // Query orders hôm nay đã thanh toán
        var todayOrders = await unitOfWork.Orders.GetByDateRangeAsync(today, tomorrow);
        var paidOrders  = todayOrders.Where(o => o.Status == OrderStatus.Paid).ToList();

        var todayRevenue = paidOrders
            .Where(o => o.Invoice is not null)
            .Sum(o => o.Invoice!.Total);

        // Trạng thái bàn
        var allTables     = (await unitOfWork.Tables.GetAllAsync()).ToList();
        var activeTables  = allTables.Count(t => t.Status == TableStatus.Occupied);
        var availTables   = allTables.Count(t => t.Status == TableStatus.Available);

        // Top menu items hôm nay
        var topItems = paidOrders
            .SelectMany(o => o.OrderItems)
            .GroupBy(oi => oi.MenuItem?.Name ?? "Unknown")
            .Select(g => new TopMenuItemDto(
                Name:          g.Key,
                TotalQuantity: g.Sum(x => x.Quantity),
                TotalRevenue:  g.Sum(x => x.UnitPrice * x.Quantity)
            ))
            .OrderByDescending(x => x.TotalQuantity)
            .Take(5)
            .ToList();

        // Doanh thu theo giờ
        var hourlyRevenue = paidOrders
            .Where(o => o.Invoice is not null)
            .GroupBy(o => o.CreatedAt.Hour)
            .Select(g => new RevenueByHourDto(
                Hour:   g.Key,
                Amount: g.Sum(o => o.Invoice!.Total)
            ))
            .OrderBy(x => x.Hour)
            .ToList();

        // Revenue growth (so với hôm qua)
        var yesterday      = today.AddDays(-1);
        var yesterdayOrders = await unitOfWork.Orders.GetByDateRangeAsync(yesterday, today);
        var yesterdayRevenue = yesterdayOrders
            .Where(o => o.Status == OrderStatus.Paid && o.Invoice is not null)
            .Sum(o => o.Invoice!.Total);

        var growth = yesterdayRevenue == 0 ? 100m
            : Math.Round((todayRevenue - yesterdayRevenue) / yesterdayRevenue * 100, 1);

        var summary = new DashboardSummaryDto(
            TodayRevenue:   todayRevenue,
            TodayOrders:    paidOrders.Count,
            ActiveTables:   activeTables,
            AvailableTables: availTables,
            RevenueGrowth:  growth,
            TopItems:       topItems,
            HourlyRevenue:  hourlyRevenue
        );

        // Cache 5 phút
        await cache.SetAsync(cacheKey, summary, TimeSpan.FromMinutes(5));
        return summary;
    }

    public async Task<RevenueReportDto> GetRevenueReportAsync(DateTime from, DateTime to)
    {
        var orders = await unitOfWork.Orders.GetByDateRangeAsync(from, to);
        var paid   = orders.Where(o => o.Status == OrderStatus.Paid && o.Invoice is not null).ToList();

        var totalRevenue = paid.Sum(o => o.Invoice!.Total);
        var avgValue     = paid.Count > 0 ? Math.Round(totalRevenue / paid.Count, 0) : 0;

        // Breakdown theo ngày
        var daily = paid
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new DailyRevenueDto(
                Date:       g.Key,
                Revenue:    g.Sum(o => o.Invoice!.Total),
                OrderCount: g.Count()
            ))
            .OrderBy(x => x.Date)
            .ToList();

        return new RevenueReportDto(
            From:              from,
            To:                to,
            TotalRevenue:      totalRevenue,
            TotalOrders:       paid.Count,
            AverageOrderValue: avgValue,
            DailyBreakdown:    daily
        );
    }
}
