using AutoMapper;
using DineFlow.Application.DTOs.Table;
using DineFlow.Domain.Entities;
using DineFlow.Domain.Enums;

namespace DineFlow.Application.Mappings;

public class TableMappingProfile : Profile
{
    public TableMappingProfile()
    {
        CreateMap<DiningTable, DiningTableDto>()
            .ForMember(
                dest => dest.StatusDisplay,
                opt => opt.MapFrom(src => GetStatusDisplay(src.Status))
            );

        CreateMap<CreateTableRequest, DiningTable>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => TableStatus.Available))
            .ForMember(dest => dest.Orders, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
    }

    private static string GetStatusDisplay(TableStatus status) => status switch
    {
        TableStatus.Available => "Trống",
        TableStatus.Occupied  => "Đang có khách",
        TableStatus.Reserved  => "Đã đặt trước",
        TableStatus.Cleaning  => "Đang dọn dẹp",
        _                     => status.ToString()
    };
}
