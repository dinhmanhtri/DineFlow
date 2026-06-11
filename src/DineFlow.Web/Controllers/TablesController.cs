using DineFlow.Web.Models;
using DineFlow.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace DineFlow.Web.Controllers;

public class TablesController(
    ITableApiClient tableApiClient) : Controller
{
    private const string SessionKeyToken = "jwt_token";
    private bool IsLoggedIn() => HttpContext.Session.GetString(SessionKeyToken) is not null;

    // GET /Tables
    public async Task<IActionResult> Index(int? floor)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");

        var tables = await tableApiClient.GetAllTablesAsync(floor);
        var floors = tables.Select(t => t.FloorNumber).Distinct().OrderBy(f => f).ToList();

        var vm = new TableIndexViewModel
        {
            Tables          = tables,
            Floors          = floors,
            FilterFloor     = floor,
            TotalTables     = tables.Count,
            AvailableTables = tables.Count(t => t.Status == 0), // Available = 0
            OccupiedTables  = tables.Count(t => t.Status == 1)  // Occupied  = 1
        };
        return View(vm);
    }

    // GET /Tables/Create
    public IActionResult Create()
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
        return View(new TableFormViewModel());
    }

    // POST /Tables/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TableFormViewModel model)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");

        if (!ModelState.IsValid) return View(model);

        var result = await tableApiClient.CreateTableAsync(model);
        if (result is null)
        {
            ModelState.AddModelError(string.Empty, "Không thể tạo bàn. Số bàn có thể đã tồn tại.");
            return View(model);
        }

        TempData["Success"] = $"Đã thêm Bàn {result.TableNumber} (Tầng {result.FloorNumber}) thành công!";
        return RedirectToAction(nameof(Index));
    }

    // POST /Tables/UpdateStatus
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(Guid id, int status)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");

        var success = await tableApiClient.UpdateTableStatusAsync(id, status);
        TempData[success ? "Success" : "Error"] = success
            ? "Đã cập nhật trạng thái bàn!"
            : "Không thể cập nhật trạng thái bàn.";

        return RedirectToAction(nameof(Index));
    }

    // POST /Tables/Delete
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");

        var success = await tableApiClient.DeleteTableAsync(id);
        TempData[success ? "Success" : "Error"] = success
            ? "Đã xóa bàn thành công!"
            : "Không thể xóa bàn (bàn đang có khách hoặc đã có order).";

        return RedirectToAction(nameof(Index));
    }
}
