using DineFlow.Web.Models;
using DineFlow.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace DineFlow.Web.Controllers;

public class OrdersController(
    IOrderApiClient  orderApiClient,
    ITableApiClient  tableApiClient,
    IMenuApiClient   menuApiClient) : Controller
{
    private const string SessionKeyToken = "jwt_token";
    private bool IsLoggedIn() => HttpContext.Session.GetString(SessionKeyToken) is not null;

    // GET /Orders
    public async Task<IActionResult> Index(int? status)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");

        var orders = await orderApiClient.GetAllOrdersAsync(status);

        var vm = new OrderIndexViewModel
        {
            Orders         = orders,
            StatusFilter   = status,
            TotalOrders    = orders.Count,
            PendingCount   = orders.Count(o => o.Status == 0),
            PreparingCount = orders.Count(o => o.Status == 1),
            ServedCount    = orders.Count(o => o.Status == 2)
        };
        return View(vm);
    }

    // GET /Orders/Detail/{id}
    public async Task<IActionResult> Detail(Guid id)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");

        var order = await orderApiClient.GetOrderByIdAsync(id);
        if (order is null) return NotFound();

        var menuItems = await menuApiClient.GetAllItemsAsync();
        var vm = new OrderDetailViewModel
        {
            Order     = order,
            MenuItems = menuItems.Where(m => m.IsAvailable)
        };
        return View(vm);
    }

    // GET /Orders/Create
    public async Task<IActionResult> Create()
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");

        // Chỉ hiển thị bàn trống (Status = 0 = Available)
        var tables = await tableApiClient.GetAllTablesAsync();
        var vm = new CreateOrderViewModel
        {
            AvailableTables = tables.Where(t => t.Status == 0)
        };
        return View(vm);
    }

    // POST /Orders/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateOrderViewModel model)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");

        if (!ModelState.IsValid)
        {
            var tables = await tableApiClient.GetAllTablesAsync();
            model.AvailableTables = tables.Where(t => t.Status == 0);
            return View(model);
        }

        var order = await orderApiClient.CreateOrderAsync(model.TableId, model.Note);
        if (order is null)
        {
            ModelState.AddModelError(string.Empty, "Không thể tạo đơn. Bàn này có thể đang có order.");
            var tables = await tableApiClient.GetAllTablesAsync();
            model.AvailableTables = tables.Where(t => t.Status == 0);
            return View(model);
        }

        TempData["Success"] = $"Đã tạo đơn hàng cho Bàn {order.TableNumber} thành công!";
        return RedirectToAction(nameof(Detail), new { id = order.Id });
    }

    // POST /Orders/AddItem
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddItem(AddOrderItemViewModel model)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");

        var order = await orderApiClient.AddItemToOrderAsync(
            model.OrderId, model.MenuItemId, model.Quantity, model.Note);

        TempData[order is not null ? "Success" : "Error"] = order is not null
            ? "Đã thêm món vào đơn hàng!"
            : "Không thể thêm món (đơn hàng không ở trạng thái hợp lệ).";

        return RedirectToAction(nameof(Detail), new { id = model.OrderId });
    }

    // POST /Orders/RemoveItem
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveItem(Guid orderId, Guid orderItemId)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");

        var success = await orderApiClient.RemoveItemFromOrderAsync(orderId, orderItemId);
        TempData[success ? "Success" : "Error"] = success
            ? "Đã xóa món khỏi đơn hàng!"
            : "Không thể xóa món.";

        return RedirectToAction(nameof(Detail), new { id = orderId });
    }

    // POST /Orders/UpdateStatus
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(Guid id, int newStatus)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");

        var success = await orderApiClient.UpdateOrderStatusAsync(id, newStatus);
        TempData[success ? "Success" : "Error"] = success
            ? "Đã cập nhật trạng thái đơn hàng!"
            : "Không thể cập nhật trạng thái.";

        return RedirectToAction(nameof(Detail), new { id });
    }

    // POST /Orders/Cancel
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(Guid id)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");

        var success = await orderApiClient.CancelOrderAsync(id);
        TempData[success ? "Success" : "Error"] = success
            ? "Đã hủy đơn hàng!"
            : "Không thể hủy đơn hàng.";

        return RedirectToAction(nameof(Index));
    }
}
