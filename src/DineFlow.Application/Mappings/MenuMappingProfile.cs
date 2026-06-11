using AutoMapper;
using DineFlow.Application.DTOs.Menu;
using DineFlow.Domain.Entities;

namespace DineFlow.Application.Mappings;

/// <summary>
/// AutoMapper Profile — định nghĩa mapping rules giữa Entity và DTO
/// 
/// [KIẾN THỨC] AutoMapper hoạt động thế nào?
/// 1. Scan tất cả Profile classes khi app start
/// 2. Build mapping configuration một lần (Singleton)
/// 3. Khi gọi _mapper.Map<DestType>(source): apply configured rules
/// 
/// Convention: nếu property name khớp (case-insensitive) → auto map
/// Cần config thêm khi:
/// - Tên khác nhau (ForMember)
/// - Computed value (MapFrom với expression)
/// - Flatten nested object (ForMember + opt.MapFrom)
/// - Ignore property (ForMember + opt.Ignore)
/// </summary>
public class MenuMappingProfile : Profile
{
    public MenuMappingProfile()
    {
        // ===== Category =====
        CreateMap<Category, CategoryDto>()
            .ForMember(
                dest => dest.MenuItemCount,
                opt => opt.MapFrom(src => src.MenuItems.Count)
                // Chỉ đúng khi MenuItems được Include() (Eager Loading)
                // Nếu không Include, sẽ là 0 (không phải null vì init = new List<>())
            );

        // CreateMap chỉ định nghĩa 1 chiều (Category → CategoryDto)
        // Để map ngược lại cần thêm ReverseMap() hoặc CreateMap<CategoryDto, Category>()

        // ===== MenuItem =====
        CreateMap<MenuItem, MenuItemDto>()
            .ForMember(
                dest => dest.CategoryName,
                opt => opt.MapFrom(src => src.Category.Name)
                // Flatten: lấy property từ navigation property
            );

        CreateMap<MenuItem, MenuItemSummaryDto>()
            .ForMember(
                dest => dest.CategoryName,
                opt => opt.MapFrom(src => src.Category.Name)
            );

        // Request → Entity (dùng khi Create)
        CreateMap<CreateMenuItemRequest, MenuItem>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())        // Server tự gen
            .ForMember(dest => dest.IsAvailable, opt => opt.MapFrom(_ => true))  // Default
            .ForMember(dest => dest.Category, opt => opt.Ignore())  // Navigation — EF tự fill
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) // BaseEntity tự set
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
    }
}
