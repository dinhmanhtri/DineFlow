using System.ComponentModel.DataAnnotations;
using DineFlow.Domain.Enums;

namespace DineFlow.Application.DTOs.Invoice;

public record InvoiceDto(
    Guid Id,
    Guid OrderId,
    int TableNumber,
    decimal SubTotal,
    decimal DiscountAmount,
    decimal TaxAmount,
    decimal Total,
    PaymentMethod PaymentMethod,
    string PaymentMethodDisplay,
    string CashierName,
    DateTime PaidAt
);

public record CreateInvoiceRequest(
    [Required] Guid OrderId,

    [Range(0, double.MaxValue, ErrorMessage = "Giảm giá không được âm")]
    decimal DiscountAmount = 0,

    [Required] PaymentMethod PaymentMethod = PaymentMethod.Cash
);
