using DineFlow.Web.Models;
using DineFlow.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace DineFlow.Web.Controllers;

public class StaffController(IStaffApiClient staffApiClient) : Controller
{
    private const string SessionKeyToken = "jwt_token";
    private bool IsLoggedIn() => HttpContext.Session.GetString(SessionKeyToken) is not null;

    // GET /Staff
    public async Task<IActionResult> Index()
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");

        var staff = await staffApiClient.GetAllStaffAsync();
        var vm = new StaffIndexViewModel
        {
            Staff       = staff,
            TotalCount  = staff.Count,
            ActiveCount = staff.Count(s => s.IsActive)
        };
        return View(vm);
    }
}
