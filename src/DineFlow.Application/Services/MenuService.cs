using AutoMapper;
using DineFlow.Application.DTOs.Menu;
using DineFlow.Application.Interfaces;
using DineFlow.Application.Interfaces.Services;
using DineFlow.Domain.Entities;
using DineFlow.Domain.Exceptions;

namespace DineFlow.Application.Services;

/// <summary>
/// MenuService — Business Logic Layer
/// 
/// [KIẾN THỨC] Cache-Aside Pattern trong Service:
/// 1. GetAll: Check cache → miss → query DB → write cache → return
/// 2. Create/Update/Delete: thực hiện xong → INVALIDATE cache (không update cache)
///    → Vì cache có thể stale nếu update trực tiếp, invalidate đảm bảo consistency
/// 
/// [KIẾN THỨC] Tại sao inject IMapper, IUnitOfWork, ICacheService?
/// - IMapper: AutoMapper instance (Singleton, thread-safe)
/// - IUnitOfWork: 1 transaction per request (Scoped)
/// - ICacheService: Redis client (Singleton connection, Scoped wrapper)
/// 
/// Tất cả inject qua constructor DI → testable (có thể mock)
/// </summary>
public class MenuService(
    IUnitOfWork unitOfWork,
    ICacheService cache,
    IMapper mapper) : IMenuService
{
    private const string CacheKeyAllCategories = "menu:categories:all";
    private const string CacheKeyMenuAll       = "menu:items:all";
    private const string CacheKeyMenuAvailable = "menu:items:available";

    // ==================== CATEGORIES ====================

    public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
    {
        // TODO: Cần ICategoryRepository — tạm thời trả về empty
        // Sẽ hoàn thiện ở Phase 4 khi có đủ repository
        return Array.Empty<CategoryDto>();
    }

    public async Task<CategoryDto> GetCategoryByIdAsync(Guid id)
    {
        // Direct DB query — category ít thay đổi, cache ở GetAll đã đủ
        var item = await unitOfWork.Menus.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Category), id);

        // Placeholder return — sẽ hoàn thiện với ICategoryRepository
        return new CategoryDto(item.Id, "Category", 1, true, 0);
    }

    public Task<CategoryDto> CreateCategoryAsync(CreateCategoryRequest request)
        => throw new NotImplementedException("Cần ICategoryRepository — sẽ thêm ở Phase 4.");

    public async Task<CategoryDto> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request)
    {
        throw new NotImplementedException("Cần ICategoryRepository");
    }

    public async Task DeleteCategoryAsync(Guid id)
    {
        throw new NotImplementedException("Cần ICategoryRepository");
    }

    // ==================== MENU ITEMS ====================

    public async Task<IEnumerable<MenuItemDto>> GetAllMenuItemsAsync()
    {
        // 1. Check cache
        var cached = await cache.GetAsync<List<MenuItemDto>>(CacheKeyMenuAll);
        if (cached is not null) return cached;

        // 2. Cache miss → query DB với Include(Category)
        var items = await unitOfWork.Menus.GetAllWithCategoryAsync();
        var dtos  = mapper.Map<List<MenuItemDto>>(items);

        // 3. Write to cache — 10 phút TTL
        await cache.SetAsync(CacheKeyMenuAll, dtos, TimeSpan.FromMinutes(10));

        return dtos;
    }

    public async Task<IEnumerable<MenuItemSummaryDto>> GetAvailableMenuItemsAsync()
    {
        var cached = await cache.GetAsync<List<MenuItemSummaryDto>>(CacheKeyMenuAvailable);
        if (cached is not null) return cached;

        var items = await unitOfWork.Menus.GetAvailableAsync();
        var dtos  = mapper.Map<List<MenuItemSummaryDto>>(items);

        await cache.SetAsync(CacheKeyMenuAvailable, dtos, TimeSpan.FromMinutes(10));
        return dtos;
    }

    public async Task<IEnumerable<MenuItemDto>> GetMenuItemsByCategoryAsync(Guid categoryId)
    {
        var cacheKey = $"menu:items:category:{categoryId}";
        var cached   = await cache.GetAsync<List<MenuItemDto>>(cacheKey);
        if (cached is not null) return cached;

        var items = await unitOfWork.Menus.GetByCategoryAsync(categoryId);
        var dtos  = mapper.Map<List<MenuItemDto>>(items);

        await cache.SetAsync(cacheKey, dtos, TimeSpan.FromMinutes(10));
        return dtos;
    }

    public async Task<MenuItemDto> GetMenuItemByIdAsync(Guid id)
    {
        var cacheKey = $"menu:items:{id}";
        var cached   = await cache.GetAsync<MenuItemDto>(cacheKey);
        if (cached is not null) return cached;

        var item = await unitOfWork.Menus.GetByIdWithCategoryAsync(id)
            ?? throw new NotFoundException(nameof(MenuItem), id);

        var dto = mapper.Map<MenuItemDto>(item);
        await cache.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(10));
        return dto;
    }

    public async Task<MenuItemDto> CreateMenuItemAsync(CreateMenuItemRequest request)
    {
        var item = mapper.Map<MenuItem>(request);

        await unitOfWork.Menus.AddAsync(item);
        await unitOfWork.SaveChangesAsync();

        // Reload với Category
        var created = await unitOfWork.Menus.GetByIdWithCategoryAsync(item.Id)
            ?? throw new InvalidOperationException("Không thể reload MenuItem vừa tạo.");

        // Invalidate tất cả menu caches
        await InvalidateMenuCachesAsync();

        return mapper.Map<MenuItemDto>(created);
    }

    public async Task<MenuItemDto> UpdateMenuItemAsync(Guid id, UpdateMenuItemRequest request)
    {
        var item = await unitOfWork.Menus.GetByIdWithCategoryAsync(id)
            ?? throw new NotFoundException(nameof(MenuItem), id);

        // Cập nhật từng property (không dùng mapper.Map vào entity có tracking — dễ gây lỗi)
        item.Name        = request.Name;
        item.Description = request.Description;
        item.Price       = request.Price;
        item.CategoryId  = request.CategoryId;
        item.ImageUrl    = request.ImageUrl;
        item.IsAvailable = request.IsAvailable;

        unitOfWork.Menus.Update(item);
        await unitOfWork.SaveChangesAsync();

        await InvalidateMenuCachesAsync($"menu:items:{id}");

        var updated = await unitOfWork.Menus.GetByIdWithCategoryAsync(id);
        return mapper.Map<MenuItemDto>(updated!);
    }

    public async Task DeleteMenuItemAsync(Guid id)
    {
        var item = await unitOfWork.Menus.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(MenuItem), id);

        unitOfWork.Menus.Delete(item);
        await unitOfWork.SaveChangesAsync();

        await InvalidateMenuCachesAsync($"menu:items:{id}");
    }

    public async Task ToggleAvailabilityAsync(Guid id)
    {
        var item = await unitOfWork.Menus.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(MenuItem), id);

        item.IsAvailable = !item.IsAvailable;
        unitOfWork.Menus.Update(item);
        await unitOfWork.SaveChangesAsync();

        await InvalidateMenuCachesAsync($"menu:items:{id}");
    }

    // ===== Private Helpers =====

    private async Task InvalidateMenuCachesAsync(params string[] additionalKeys)
    {
        // Xóa tất cả menu cache keys liên quan
        await cache.RemoveAsync(CacheKeyMenuAll);
        await cache.RemoveAsync(CacheKeyMenuAvailable);
        await cache.RemoveByPatternAsync("menu:items:category:*");

        foreach (var key in additionalKeys)
            await cache.RemoveAsync(key);
    }
}
