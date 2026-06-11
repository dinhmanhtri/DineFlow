using System.Diagnostics;
using DineFlow.Web.Models;
using DineFlow.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace DineFlow.Web.Controllers;

public class HomeController(IDashboardApiClient dashboardApiClient) : Controller
{
    private const string SessionKeyToken = "jwt_token";

    public async Task<IActionResult> Index()
    {
        if (HttpContext.Session.GetString(SessionKeyToken) is null)
            return RedirectToAction("Login", "Auth");

        // Lấy KPI thực từ API — nếu lỗi, hiển thị empty dashboard
        var summary = await dashboardApiClient.GetTodaySummaryAsync()
            ?? new DashboardSummaryViewModel();

        return View(summary);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
