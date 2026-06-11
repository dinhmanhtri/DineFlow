using DineFlow.Application.DTOs.Order;
using DineFlow.Domain.Enums;

namespace DineFlow.Application.Interfaces.Services;

public interface IOrderService
{
    Task<IEnumerable<OrderSummaryDto>> GetAllOrdersAsync();
    Task<IEnumerable<OrderSummaryDto>> GetOrdersByStatusAsync(OrderStatus status);
    Task<OrderDto> GetOrderByIdAsync(Guid id);
    Task<OrderDto?> GetActiveOrderByTableAsync(Guid tableId);
    Task<OrderDto> CreateOrderAsync(CreateOrderRequest request, Guid staffId);
    Task<OrderDto> AddItemToOrderAsync(Guid orderId, AddOrderItemRequest request);
    Task RemoveItemFromOrderAsync(Guid orderId, Guid orderItemId);
    Task<OrderDto> UpdateOrderStatusAsync(Guid orderId, UpdateOrderStatusRequest request);
    Task CancelOrderAsync(Guid orderId);
}
