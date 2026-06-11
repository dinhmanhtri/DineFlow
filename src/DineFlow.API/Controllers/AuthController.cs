using DineFlow.Application.DTOs.Auth;
using DineFlow.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DineFlow.API.Controllers;

/// <summary>
/// AuthController — Đăng nhập, đăng ký và quản lý nhân viên
/// 
/// [KIẾN THỨC] [AllowAnonymous] vs [Authorize]:
/// - [Authorize] ở class-level: tất cả actions cần JWT
/// - [AllowAnonymous] ở action-level: override class-level → không cần JWT
/// → Login phải AllowAnonymous (user chưa có token nên không thể gửi token!)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AuthController(IAuthService authService) : ControllerBase
{
    /// <summary>Đăng nhập và nhận JWT token</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var response = await authService.LoginAsync(request);
        return Ok(response);
    }

    /// <summary>Đăng ký nhân viên mới (Admin only)</summary>
    [HttpPost("register")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(StaffDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Register([FromBody] RegisterStaffRequest request)
    {
        var staff = await authService.RegisterStaffAsync(request);
        return CreatedAtAction(nameof(GetStaffById), new { id = staff.Id }, staff);
    }

    /// <summary>Lấy thông tin của staff hiện tại (từ JWT claims)</summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(StaffDto), 200)]
    public async Task<IActionResult> GetCurrentStaff()
    {
        // Lấy userId từ JWT claims
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdStr, out var userId))
            return Unauthorized();

        var staff = await authService.GetStaffByIdAsync(userId);
        return Ok(staff);
    }

    /// <summary>Lấy danh sách tất cả nhân viên (Admin only)</summary>
    [HttpGet("staff")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IEnumerable<StaffDto>), 200)]
    public async Task<IActionResult> GetAllStaff()
    {
        var staff = await authService.GetAllStaffAsync();
        return Ok(staff);
    }

    /// <summary>Lấy thông tin nhân viên theo Id (Admin only)</summary>
    [HttpGet("staff/{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(StaffDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetStaffById(Guid id)
    {
        var staff = await authService.GetStaffByIdAsync(id);
        return Ok(staff);
    }

    /// <summary>Bật/tắt trạng thái nhân viên (Admin only)</summary>
    [HttpPatch("staff/{id:guid}/toggle-status")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(StaffDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ToggleStaffStatus(Guid id)
    {
        var staff = await authService.ToggleStaffStatusAsync(id);
        return Ok(staff);
    }
}
