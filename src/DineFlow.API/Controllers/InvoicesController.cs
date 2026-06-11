using DineFlow.Application.DTOs.Invoice;
using DineFlow.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DineFlow.API.Controllers;

[ApiController]
[Route("api/invoices")]
[Authorize]
public class InvoicesController(IInvoiceService invoiceService) : ControllerBase
{
    [HttpGet("by-order/{orderId:guid}")]
    [ProducesResponseType(typeof(InvoiceDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetByOrder(Guid orderId)
    {
        var invoice = await invoiceService.GetInvoiceByOrderIdAsync(orderId);
        return Ok(invoice);
    }

    [HttpGet("by-date")]
    [ProducesResponseType(typeof(IEnumerable<InvoiceDto>), 200)]
    public async Task<IActionResult> GetByDate([FromQuery] DateTime date)
    {
        var invoices = await invoiceService.GetInvoicesByDateAsync(date);
        return Ok(invoices);
    }

    /// <summary>Thanh toán order (tạo invoice)</summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Cashier")]
    [ProducesResponseType(typeof(InvoiceDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> CreateInvoice([FromBody] CreateInvoiceRequest request)
    {
        var cashierIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(cashierIdStr, out var cashierId))
            return Unauthorized();

        var invoice = await invoiceService.CreateInvoiceAsync(request, cashierId);
        return CreatedAtAction(nameof(GetByOrder), new { orderId = invoice.OrderId }, invoice);
    }
}
