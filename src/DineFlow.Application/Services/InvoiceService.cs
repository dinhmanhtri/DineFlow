using AutoMapper;
using DineFlow.Application.DTOs.Invoice;
using DineFlow.Application.Interfaces;
using DineFlow.Application.Interfaces.Services;
using DineFlow.Domain.Entities;
using DineFlow.Domain.Enums;
using DineFlow.Domain.Exceptions;

namespace DineFlow.Application.Services;

public class InvoiceService(
    IUnitOfWork unitOfWork,
    IMapper mapper) : IInvoiceService
{
    private const decimal VatRate = 0.08m; // 8% VAT

    public async Task<InvoiceDto> GetInvoiceByOrderIdAsync(Guid orderId)
    {
        var invoice = await unitOfWork.Orders.GetByIdWithDetailsAsync(orderId);
        if (invoice?.Invoice is null)
            throw new NotFoundException("Invoice for Order", orderId);

        return mapper.Map<InvoiceDto>(invoice.Invoice);
    }

    public async Task<InvoiceDto> CreateInvoiceAsync(CreateInvoiceRequest request, Guid cashierId)
    {
        var order = await unitOfWork.Orders.GetByIdWithDetailsAsync(request.OrderId)
            ?? throw new NotFoundException(nameof(Order), request.OrderId);

        if (order.Status == OrderStatus.Paid)
            throw new ConflictException("Order này đã được thanh toán.");

        if (order.Status == OrderStatus.Cancelled)
            throw new ValidationException("OrderId", "Không thể tạo hóa đơn cho order đã hủy.");

        // Tính thuế
        var subTotal     = order.TotalAmount;
        var vatAmount    = Math.Round(subTotal * VatRate, 0);
        var total        = subTotal - request.DiscountAmount + vatAmount;

        // [KIẾN THỨC] Factory Method trong Domain:
        // Invoice.CreateFromOrder() chứa logic khởi tạo → Service không cần biết chi tiết
        var invoice = Invoice.CreateFromOrder(
            order:          order,
            discountAmount: request.DiscountAmount,
            cashierId:      cashierId,
            paymentMethod:  request.PaymentMethod
        );

        await unitOfWork.BeginTransactionAsync();
        try
        {
            // Update order status → Paid
            order.Status = OrderStatus.Paid;
            unitOfWork.Orders.Update(order);

            // Giải phóng bàn
            var table = await unitOfWork.Tables.GetByIdAsync(order.TableId);
            if (table is not null)
            {
                table.Status = TableStatus.Available;
                unitOfWork.Tables.Update(table);
            }

            await unitOfWork.CommitTransactionAsync();
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync();
            throw;
        }

        return mapper.Map<InvoiceDto>(invoice);
    }

    public async Task<IEnumerable<InvoiceDto>> GetInvoicesByDateAsync(DateTime date)
    {
        var from   = date.Date;
        var to     = date.Date.AddDays(1).AddSeconds(-1);
        var orders = await unitOfWork.Orders.GetByDateRangeAsync(from, to);

        var invoices = orders
            .Where(o => o.Invoice is not null)
            .Select(o => o.Invoice!);

        return mapper.Map<IEnumerable<InvoiceDto>>(invoices);
    }
}
