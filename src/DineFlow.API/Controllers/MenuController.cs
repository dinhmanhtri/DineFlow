using DineFlow.Application.DTOs.Menu;
using DineFlow.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DineFlow.API.Controllers;

/// <summary>
/// MenuController — Quản lý danh mục và món ăn
/// 
/// [KIẾN THỨC] RESTful API Conventions:
/// GET    /api/menu/items         → 200 + list
/// GET    /api/menu/items/{id}    → 200 + item | 404
/// POST   /api/menu/items         → 201 + item (kèm Location header)
/// PUT    /api/menu/items/{id}    → 204 No Content
/// DELETE /api/menu/items/{id}    → 204 No Content
/// PATCH  /api/menu/items/{id}/.. → 200 + updated item
/// 
/// [KIẾN THỨC] CreatedAtAction:
/// Trả về 201 Created + Location header trỏ tới URL của resource mới
/// Client có thể GET ngay theo URL đó
/// </summary>
[ApiController]
[Route("api/menu")]
[Authorize]
public class MenuController(IMenuService menuService) : ControllerBase
{
    // ==================== CATEGORIES ====================

    [HttpGet("categories")]
    [AllowAnonymous]  // Menu visible cho tất cả (kể cả chưa đăng nhập)
    [ProducesResponseType(typeof(IEnumerable<CategoryDto>), 200)]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await menuService.GetAllCategoriesAsync();
        return Ok(categories);
    }

    [HttpGet("categories/{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CategoryDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetCategory(Guid id)
    {
        var category = await menuService.GetCategoryByIdAsync(id);
        return Ok(category);
    }

    [HttpPost("categories")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(CategoryDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        var category = await menuService.CreateCategoryAsync(request);
        return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
    }

    [HttpPut("categories/{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(CategoryDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateCategoryRequest request)
    {
        var category = await menuService.UpdateCategoryAsync(id, request);
        return Ok(category);
    }

    [HttpDelete("categories/{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        await menuService.DeleteCategoryAsync(id);
        return NoContent();
    }

    // ==================== MENU ITEMS ====================

    [HttpGet("items")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<MenuItemDto>), 200)]
    public async Task<IActionResult> GetMenuItems([FromQuery] Guid? categoryId)
    {
        var items = categoryId.HasValue
            ? await menuService.GetMenuItemsByCategoryAsync(categoryId.Value)
            : await menuService.GetAllMenuItemsAsync();
        return Ok(items);
    }

    [HttpGet("items/available")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<MenuItemSummaryDto>), 200)]
    public async Task<IActionResult> GetAvailableItems()
    {
        var items = await menuService.GetAvailableMenuItemsAsync();
        return Ok(items);
    }

    [HttpGet("items/{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(MenuItemDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetMenuItem(Guid id)
    {
        var item = await menuService.GetMenuItemByIdAsync(id);
        return Ok(item);
    }

    [HttpPost("items")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(MenuItemDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateMenuItem([FromBody] CreateMenuItemRequest request)
    {
        var item = await menuService.CreateMenuItemAsync(request);
        return CreatedAtAction(nameof(GetMenuItem), new { id = item.Id }, item);
    }

    [HttpPut("items/{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(MenuItemDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateMenuItem(Guid id, [FromBody] UpdateMenuItemRequest request)
    {
        var item = await menuService.UpdateMenuItemAsync(id, request);
        return Ok(item);
    }

    [HttpDelete("items/{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteMenuItem(Guid id)
    {
        await menuService.DeleteMenuItemAsync(id);
        return NoContent();
    }

    [HttpPatch("items/{id:guid}/toggle-availability")]
    [Authorize(Roles = "Admin,Waiter")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ToggleAvailability(Guid id)
    {
        await menuService.ToggleAvailabilityAsync(id);
        return NoContent();
    }
}
