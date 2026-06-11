using DineFlow.Application.DTOs.Dashboard;
using DineFlow.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DineFlow.API.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController(IDashboardService dashboardService) : ControllerBase
{
    /// <summary>KPI tổng quan hôm nay</summary>
    [HttpGet("today")]
    [ProducesResponseType(typeof(DashboardSummaryDto), 200)]
    public async Task<IActionResult> GetTodaySummary()
    {
        var summary = await dashboardService.GetTodaySummaryAsync();
        return Ok(summary);
    }

    /// <summary>Báo cáo doanh thu theo khoảng thời gian</summary>
    [HttpGet("revenue")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(RevenueReportDto), 200)]
    public async Task<IActionResult> GetRevenueReport(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to)
    {
        if (from > to)
            return BadRequest(new { message = "Ngày bắt đầu phải trước ngày kết thúc." });

        var report = await dashboardService.GetRevenueReportAsync(from, to);
        return Ok(report);
    }
}
