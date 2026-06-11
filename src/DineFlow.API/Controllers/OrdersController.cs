using DineFlow.Application.DTOs.Order;
using DineFlow.Application.Interfaces.Services;
using DineFlow.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DineFlow.API.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController(IOrderService orderService) : ControllerBase
{
    /// <summary>Lấy tất cả orders hoặc lọc theo status</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrderSummaryDto>), 200)]
    public async Task<IActionResult> GetOrders([FromQuery] OrderStatus? status)
    {
        var orders = status.HasValue
            ? await orderService.GetOrdersByStatusAsync(status.Value)
            : await orderService.GetAllOrdersAsync();
        return Ok(orders);
    }

    /// <summary>Lấy chi tiết order theo Id</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetOrder(Guid id)
    {
        var order = await orderService.GetOrderByIdAsync(id);
        return Ok(order);
    }

    /// <summary>Lấy order đang active của bàn</summary>
    [HttpGet("by-table/{tableId:guid}")]
    [ProducesResponseType(typeof(OrderDto), 200)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> GetActiveOrderByTable(Guid tableId)
    {
        var order = await orderService.GetActiveOrderByTableAsync(tableId);
        return order is null ? NoContent() : Ok(order);
    }

    /// <summary>Tạo order mới (gán staffId từ JWT)</summary>
    [HttpPost]
    [ProducesResponseType(typeof(OrderDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        // Lấy staffId từ JWT token — không cần client gửi
        var staffIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(staffIdStr, out var staffId))
            return Unauthorized();

        var order = await orderService.CreateOrderAsync(request, staffId);
        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
    }

    /// <summary>Thêm món vào order</summary>
    [HttpPost("{id:guid}/items")]
    [ProducesResponseType(typeof(OrderDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> AddItem(Guid id, [FromBody] AddOrderItemRequest request)
    {
        var order = await orderService.AddItemToOrderAsync(id, request);
        return Ok(order);
    }

    /// <summary>Xóa món khỏi order</summary>
    [HttpDelete("{id:guid}/items/{itemId:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> RemoveItem(Guid id, Guid itemId)
    {
        await orderService.RemoveItemFromOrderAsync(id, itemId);
        return NoContent();
    }

    /// <summary>Cập nhật trạng thái order (Pending → Preparing → Served)</summary>
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(OrderDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
    {
        var order = await orderService.UpdateOrderStatusAsync(id, request);
        return Ok(order);
    }

    /// <summary>Hủy order</summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> CancelOrder(Guid id)
    {
        await orderService.CancelOrderAsync(id);
        return NoContent();
    }
}
