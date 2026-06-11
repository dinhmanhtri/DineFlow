using DineFlow.Web.Models;
using DineFlow.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace DineFlow.Web.Controllers;

public class InvoicesController(
    IInvoiceApiClient invoiceApiClient,
    IOrderApiClient   orderApiClient) : Controller
{
    private const string SessionKeyToken = "jwt_token";
    private bool IsLoggedIn() => HttpContext.Session.GetString(SessionKeyToken) is not null;

    // GET /Invoices
    public async Task<IActionResult> Index()
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");

        var invoices = await invoiceApiClient.GetTodayInvoicesAsync();
        var vm = new InvoiceIndexViewModel
        {
            Invoices     = invoices,
            TodayRevenue = invoices.Sum(i => i.Total),
            TodayCount   = invoices.Count,
            Date         = DateTime.Today
        };
        return View(vm);
    }

    // GET /Invoices/Detail/{orderId} — xem hóa đơn của order
    public async Task<IActionResult> Detail(Guid orderId)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");

        var invoice = await invoiceApiClient.GetInvoiceByOrderAsync(orderId);
        if (invoice is null) return NotFound();

        return View(invoice);
    }

    // GET /Invoices/Create/{orderId} — form thanh toán
    public async Task<IActionResult> Create(Guid orderId)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");

        var order = await orderApiClient.GetOrderByIdAsync(orderId);
        if (order is null) return NotFound();

        // Chỉ cho tạo hóa đơn khi order ở trạng thái Served (2)
        if (order.Status != 2)
        {
            TempData["Error"] = "Chỉ có thể thanh toán khi đơn hàng đã được phục vụ xong.";
            return RedirectToAction("Detail", "Orders", new { id = orderId });
        }

        var taxRate = 0.08m; // 8% VAT
        var vm = new CreateInvoiceViewModel
        {
            OrderId     = orderId,
            TableNumber = order.TableNumber,
            SubTotal    = order.TotalAmount,
            TaxAmount   = Math.Round(order.TotalAmount * taxRate, 0)
        };
        return View(vm);
    }

    // POST /Invoices/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateInvoiceViewModel model)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");

        if (!ModelState.IsValid) return View(model);

        var invoice = await invoiceApiClient.CreateInvoiceAsync(
            model.OrderId, model.DiscountAmount, model.PaymentMethod);

        if (invoice is null)
        {
            ModelState.AddModelError(string.Empty, "Không thể tạo hóa đơn. Đơn hàng có thể đã được thanh toán.");
            return View(model);
        }

        TempData["Success"] = $"Thanh toán thành công! Tổng: {invoice.Total:N0} VND";
        return RedirectToAction(nameof(Detail), new { orderId = invoice.OrderId });
    }
}
