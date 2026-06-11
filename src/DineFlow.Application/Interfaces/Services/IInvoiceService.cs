using DineFlow.Application.DTOs.Invoice;

namespace DineFlow.Application.Interfaces.Services;

public interface IInvoiceService
{
    Task<InvoiceDto> GetInvoiceByOrderIdAsync(Guid orderId);
    Task<InvoiceDto> CreateInvoiceAsync(CreateInvoiceRequest request, Guid cashierId);
    Task<IEnumerable<InvoiceDto>> GetInvoicesByDateAsync(DateTime date);
}
