using System.Diagnostics;
using DineFlow.Web.Models;
using DineFlow.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace DineFlow.Web.Controllers;

public class HomeController(IMenuApiClient menuApiClient) : Controller
{
    private const string SessionKeyToken = "jwt_token";

    public async Task<IActionResult> Index()
    {
        if (HttpContext.Session.GetString(SessionKeyToken) is null)
            return RedirectToAction("Login", "Auth");

        var allItems = await menuApiClient.GetAllItemsAsync();

        var vm = new DashboardViewModel
        {
            TotalMenuItems   = allItems.Count,
            AvailableItems   = allItems.Count(i => i.IsAvailable),
            UnavailableItems = allItems.Count(i => !i.IsAvailable)
        };

        return View(vm);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
