using AutoMapper;
using DineFlow.Application.DTOs.Invoice;
using DineFlow.Application.DTOs.Order;
using DineFlow.Domain.Entities;
using DineFlow.Domain.Enums;

namespace DineFlow.Application.Mappings;

public class OrderMappingProfile : Profile
{
    public OrderMappingProfile()
    {
        // OrderItem → OrderItemDto
        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(dest => dest.MenuItemName,
                opt => opt.MapFrom(src => src.MenuItem.Name))
            .ForMember(dest => dest.SubTotal,
                opt => opt.MapFrom(src => src.UnitPrice * src.Quantity));

        // Order → OrderDto (full detail)
        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.TableNumber,
                opt => opt.MapFrom(src => src.Table.TableNumber))
            .ForMember(dest => dest.FloorNumber,
                opt => opt.MapFrom(src => src.Table.FloorNumber))
            .ForMember(dest => dest.StaffName,
                opt => opt.MapFrom(src => src.Staff.FullName))
            .ForMember(dest => dest.StatusDisplay,
                opt => opt.MapFrom(src => GetStatusDisplay(src.Status)))
            .ForMember(dest => dest.Items,
                opt => opt.MapFrom(src => src.OrderItems));

        // Order → OrderSummaryDto (list view, no items)
        CreateMap<Order, OrderSummaryDto>()
            .ForMember(dest => dest.TableNumber,
                opt => opt.MapFrom(src => src.Table.TableNumber))
            .ForMember(dest => dest.StaffName,
                opt => opt.MapFrom(src => src.Staff.FullName))
            .ForMember(dest => dest.StatusDisplay,
                opt => opt.MapFrom(src => GetStatusDisplay(src.Status)))
            .ForMember(dest => dest.ItemCount,
                opt => opt.MapFrom(src => src.OrderItems.Count));
    }

    private static string GetStatusDisplay(OrderStatus status) => status switch
    {
        OrderStatus.Pending   => "Chờ xử lý",
        OrderStatus.Preparing => "Đang chuẩn bị",
        OrderStatus.Served    => "Đã phục vụ",
        OrderStatus.Paid      => "Đã thanh toán",
        OrderStatus.Cancelled => "Đã hủy",
        _                     => status.ToString()
    };
}

public class InvoiceMappingProfile : Profile
{
    public InvoiceMappingProfile()
    {
        CreateMap<Invoice, InvoiceDto>()
            .ForMember(dest => dest.TableNumber,
                opt => opt.MapFrom(src => src.Order.Table.TableNumber))
            .ForMember(dest => dest.CashierName,
                opt => opt.MapFrom(src => src.Cashier.FullName))
            .ForMember(dest => dest.PaymentMethodDisplay,
                opt => opt.MapFrom(src => GetPaymentDisplay(src.PaymentMethod)));
    }

    private static string GetPaymentDisplay(PaymentMethod method) => method switch
    {
        PaymentMethod.Cash         => "Tiền mặt",
        PaymentMethod.BankTransfer => "Chuyển khoản",
        PaymentMethod.Card         => "Thẻ",
        _                          => method.ToString()
    };
}
