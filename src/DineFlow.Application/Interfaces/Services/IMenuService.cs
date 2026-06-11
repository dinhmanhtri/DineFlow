using DineFlow.Application.DTOs.Menu;

namespace DineFlow.Application.Interfaces.Services;

/// <summary>
/// Interface cho Menu Service — business logic layer
/// 
/// [KIẾN THỨC] Service Layer pattern:
/// Controller gọi Service (không gọi Repository trực tiếp)
/// Service gọi Repository qua UnitOfWork
/// → Controller chỉ biết Service interface, không biết DB hay cache tồn tại
/// 
/// Các method trả về DTO (không phải Entity):
/// → Tách biệt domain model với API response
/// → Entity có thể thay đổi mà không ảnh hưởng API contract
/// </summary>
public interface IMenuService
{
    // ===== Categories =====
    Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync();
    Task<CategoryDto> GetCategoryByIdAsync(Guid id);
    Task<CategoryDto> CreateCategoryAsync(CreateCategoryRequest request);
    Task<CategoryDto> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request);
    Task DeleteCategoryAsync(Guid id);

    // ===== Menu Items =====
    Task<IEnumerable<MenuItemDto>> GetAllMenuItemsAsync();
    Task<IEnumerable<MenuItemSummaryDto>> GetAvailableMenuItemsAsync();
    Task<IEnumerable<MenuItemDto>> GetMenuItemsByCategoryAsync(Guid categoryId);
    Task<MenuItemDto> GetMenuItemByIdAsync(Guid id);
    Task<MenuItemDto> CreateMenuItemAsync(CreateMenuItemRequest request);
    Task<MenuItemDto> UpdateMenuItemAsync(Guid id, UpdateMenuItemRequest request);
    Task DeleteMenuItemAsync(Guid id);
    Task ToggleAvailabilityAsync(Guid id);
}
