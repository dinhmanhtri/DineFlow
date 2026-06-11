using DineFlow.Web.Models;
using DineFlow.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace DineFlow.Web.Controllers;

/// <summary>
/// MenuController — CRUD Món ăn
/// 
/// [KIẾN THỨC] PRG Pattern (Post-Redirect-Get):
/// - Sau khi POST (Create/Edit/Delete) → Redirect về GET (Index)
/// - Tránh double-submit khi user refresh trang
/// → Luôn return RedirectToAction sau thành công POST
/// 
/// [KIẾN THỨC] TempData:
/// - Dùng để truyền thông báo (toast) qua redirect
/// - TempData chỉ tồn tại 1 request sau khi được set
/// </summary>
public class MenuController(IMenuApiClient menuApiClient) : Controller
{
    private const string SessionKeyToken = "jwt_token";

    // Kiểm tra đã login chưa
    private bool IsLoggedIn() =>
        HttpContext.Session.GetString(SessionKeyToken) is not null;

    // GET /Menu
    public async Task<IActionResult> Index(string? search, Guid? categoryId)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");

        var items      = await menuApiClient.GetAllItemsAsync();
        var categories = await menuApiClient.GetCategoriesAsync();

        // Filter phía client (API chưa có search endpoint)
        if (!string.IsNullOrWhiteSpace(search))
            items = items.Where(i =>
                i.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                (i.Description?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();

        if (categoryId.HasValue)
            items = items.Where(i => i.CategoryId == categoryId.Value).ToList();

        var vm = new MenuIndexViewModel
        {
            Items      = items,
            Categories = categories,
            SearchTerm = search,
            CategoryId = categoryId,
            TotalCount = items.Count
        };

        return View(vm);
    }

    // GET /Menu/Create
    public async Task<IActionResult> Create()
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");

        var categories = await menuApiClient.GetCategoriesAsync();
        var vm         = new MenuItemFormViewModel { Categories = categories };
        return View(vm);
    }

    // POST /Menu/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MenuItemFormViewModel model)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");

        if (!ModelState.IsValid)
        {
            model.Categories = await menuApiClient.GetCategoriesAsync();
            return View(model);
        }

        var result = await menuApiClient.CreateItemAsync(model);
        if (result is null)
        {
            ModelState.AddModelError(string.Empty, "Có lỗi xảy ra khi tạo món ăn. Vui lòng thử lại.");
            model.Categories = await menuApiClient.GetCategoriesAsync();
            return View(model);
        }

        TempData["Success"] = $"Đã thêm món \"{result.Name}\" thành công!";
        return RedirectToAction(nameof(Index));
    }

    // GET /Menu/Edit/{id}
    public async Task<IActionResult> Edit(Guid id)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");

        var item = await menuApiClient.GetItemByIdAsync(id);
        if (item is null) return NotFound();

        var categories = await menuApiClient.GetCategoriesAsync();
        var vm = new MenuItemFormViewModel
        {
            Name        = item.Name,
            Description = item.Description,
            Price       = item.Price,
            CategoryId  = item.CategoryId,
            ImageUrl    = item.ImageUrl,
            IsAvailable = item.IsAvailable,
            Categories  = categories
        };

        ViewData["ItemId"] = id;
        return View(vm);
    }

    // POST /Menu/Edit/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, MenuItemFormViewModel model)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");

        if (!ModelState.IsValid)
        {
            model.Categories   = await menuApiClient.GetCategoriesAsync();
            ViewData["ItemId"] = id;
            return View(model);
        }

        var result = await menuApiClient.UpdateItemAsync(id, model);
        if (result is null)
        {
            ModelState.AddModelError(string.Empty, "Có lỗi xảy ra khi cập nhật. Vui lòng thử lại.");
            model.Categories   = await menuApiClient.GetCategoriesAsync();
            ViewData["ItemId"] = id;
            return View(model);
        }

        TempData["Success"] = $"Đã cập nhật món \"{result.Name}\" thành công!";
        return RedirectToAction(nameof(Index));
    }

    // POST /Menu/Delete/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");

        var success = await menuApiClient.DeleteItemAsync(id);
        TempData[success ? "Success" : "Error"] = success
            ? "Đã xóa món ăn thành công!"
            : "Không thể xóa món ăn. Vui lòng thử lại.";

        return RedirectToAction(nameof(Index));
    }

    // POST /Menu/Toggle/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(Guid id)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");

        var success = await menuApiClient.ToggleAvailabilityAsync(id);
        TempData[success ? "Success" : "Error"] = success
            ? "Đã cập nhật trạng thái món ăn!"
            : "Không thể cập nhật trạng thái.";

        return RedirectToAction(nameof(Index));
    }
}
