using DineFlow.Application.DTOs.Dashboard;

namespace DineFlow.Application.Interfaces.Services;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetTodaySummaryAsync();
    Task<RevenueReportDto> GetRevenueReportAsync(DateTime from, DateTime to);
}
