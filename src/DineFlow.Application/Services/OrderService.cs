using AutoMapper;
using DineFlow.Application.DTOs.Order;
using DineFlow.Application.Interfaces;
using DineFlow.Application.Interfaces.Services;
using DineFlow.Domain.Entities;
using DineFlow.Domain.Enums;
using DineFlow.Domain.Exceptions;

namespace DineFlow.Application.Services;

public class OrderService(
    IUnitOfWork unitOfWork,
    IMapper mapper) : IOrderService
{
    public async Task<IEnumerable<OrderSummaryDto>> GetAllOrdersAsync()
    {
        var orders = await unitOfWork.Orders.GetAllWithDetailsAsync();
        return mapper.Map<IEnumerable<OrderSummaryDto>>(orders);
    }

    public async Task<IEnumerable<OrderSummaryDto>> GetOrdersByStatusAsync(OrderStatus status)
    {
        var orders = await unitOfWork.Orders.GetByStatusAsync(status);
        return mapper.Map<IEnumerable<OrderSummaryDto>>(orders);
    }

    public async Task<OrderDto> GetOrderByIdAsync(Guid id)
    {
        var order = await unitOfWork.Orders.GetByIdWithDetailsAsync(id)
            ?? throw new NotFoundException(nameof(Order), id);

        return mapper.Map<OrderDto>(order);
    }

    public async Task<OrderDto?> GetActiveOrderByTableAsync(Guid tableId)
    {
        var order = await unitOfWork.Orders.GetActiveOrderByTableAsync(tableId);
        return order is null ? null : mapper.Map<OrderDto>(order);
    }

    public async Task<OrderDto> CreateOrderAsync(CreateOrderRequest request, Guid staffId)
    {
        // Kiểm tra bàn tồn tại
        var table = await unitOfWork.Tables.GetByIdAsync(request.TableId)
            ?? throw new NotFoundException(nameof(DiningTable), request.TableId);

        // Kiểm tra bàn có đang active order không
        var existing = await unitOfWork.Orders.GetActiveOrderByTableAsync(request.TableId);
        if (existing is not null)
            throw new ConflictException($"Bàn {table.TableNumber} đang có order chưa hoàn tất.");

        // [KIẾN THỨC] Transaction: tạo Order + update Table.Status phải atomic
        await unitOfWork.BeginTransactionAsync();
        try
        {
            var order = new Order
            {
                TableId = request.TableId,
                StaffId = staffId,
                Status  = OrderStatus.Pending
            };

            // Thêm items ban đầu (nếu có)
            if (request.InitialItems?.Count > 0)
            {
                foreach (var item in request.InitialItems)
                {
                    var menuItem = await unitOfWork.Menus.GetByIdAsync(item.MenuItemId)
                        ?? throw new NotFoundException(nameof(MenuItem), item.MenuItemId);

                    // Domain method — validates business rules
                    order.AddItem(menuItem.Id, menuItem.Price, item.Quantity, item.Note);
                }
            }

            await unitOfWork.Orders.AddAsync(order);

            // Cập nhật trạng thái bàn → Occupied
            table.Status = TableStatus.Occupied;
            unitOfWork.Tables.Update(table);

            await unitOfWork.CommitTransactionAsync();

            // Reload với full details để map
            var created = await unitOfWork.Orders.GetByIdWithDetailsAsync(order.Id);
            return mapper.Map<OrderDto>(created!);
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<OrderDto> AddItemToOrderAsync(Guid orderId, AddOrderItemRequest request)
    {
        var order = await unitOfWork.Orders.GetByIdWithDetailsAsync(orderId)
            ?? throw new NotFoundException(nameof(Order), orderId);

        var menuItem = await unitOfWork.Menus.GetByIdAsync(request.MenuItemId)
            ?? throw new NotFoundException(nameof(MenuItem), request.MenuItemId);

        if (!menuItem.IsAvailable)
            throw new ValidationException("MenuItemId", $"Món '{menuItem.Name}' hiện không phục vụ.");

        // Domain method handles business rules (status check, quantity validation)
        order.AddItem(menuItem.Id, menuItem.Price, request.Quantity, request.Note);

        unitOfWork.Orders.Update(order);
        await unitOfWork.SaveChangesAsync();

        var updated = await unitOfWork.Orders.GetByIdWithDetailsAsync(orderId);
        return mapper.Map<OrderDto>(updated!);
    }

    public async Task RemoveItemFromOrderAsync(Guid orderId, Guid orderItemId)
    {
        var order = await unitOfWork.Orders.GetByIdWithDetailsAsync(orderId)
            ?? throw new NotFoundException(nameof(Order), orderId);

        // Domain method
        order.RemoveItem(orderItemId);

        unitOfWork.Orders.Update(order);
        await unitOfWork.SaveChangesAsync();
    }

    public async Task<OrderDto> UpdateOrderStatusAsync(Guid orderId, UpdateOrderStatusRequest request)
    {
        var order = await unitOfWork.Orders.GetByIdWithDetailsAsync(orderId)
            ?? throw new NotFoundException(nameof(Order), orderId);

        order.Status = request.NewStatus;
        unitOfWork.Orders.Update(order);
        await unitOfWork.SaveChangesAsync();

        var updated = await unitOfWork.Orders.GetByIdWithDetailsAsync(orderId);
        return mapper.Map<OrderDto>(updated!);
    }

    public async Task CancelOrderAsync(Guid orderId)
    {
        var order = await unitOfWork.Orders.GetByIdWithDetailsAsync(orderId)
            ?? throw new NotFoundException(nameof(Order), orderId);

        if (order.Status == OrderStatus.Paid)
            throw new ValidationException("Status", "Không thể hủy order đã thanh toán.");

        order.Status = OrderStatus.Cancelled;

        // Giải phóng bàn
        var table = await unitOfWork.Tables.GetByIdAsync(order.TableId);
        if (table is not null)
        {
            table.Status = TableStatus.Available;
            unitOfWork.Tables.Update(table);
        }

        unitOfWork.Orders.Update(order);
        await unitOfWork.SaveChangesAsync();
    }
}
