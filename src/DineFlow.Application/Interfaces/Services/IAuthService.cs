using DineFlow.Application.DTOs.Auth;

namespace DineFlow.Application.Interfaces.Services;

/// <summary>
/// Auth service — xử lý login, register, JWT generation
/// Nằm trong Application layer → không depend vào HTTP context
/// </summary>
public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<StaffDto> RegisterStaffAsync(RegisterStaffRequest request, CancellationToken cancellationToken = default);
    Task<StaffDto> GetStaffByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<StaffDto>> GetAllStaffAsync(CancellationToken cancellationToken = default);
    Task<StaffDto> ToggleStaffStatusAsync(Guid id, CancellationToken cancellationToken = default);
}
