using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DineFlow.Application.DTOs.Auth;
using DineFlow.Application.Interfaces;
using DineFlow.Application.Interfaces.Services;
using DineFlow.Domain.Entities;
using DineFlow.Domain.Enums;
using DineFlow.Domain.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DineFlow.Infrastructure.Services;

/// <summary>
/// AuthService — Xác thực và cấp JWT
/// 
/// [KIẾN THỨC] Quy trình đăng nhập JWT:
/// 1. Client gửi email + password
/// 2. Server tìm Staff theo email trong DB
/// 3. BCrypt.Verify(password, storedHash) → true/false
/// 4. Nếu đúng: tạo JWT với claims (userId, role, email)
/// 5. Client lưu token → gửi kèm mọi request sau
/// 6. Server verify token signature → biết user hợp lệ mà không cần query DB
/// 
/// [KIẾN THỨC] Claims trong JWT:
/// - ClaimTypes.NameIdentifier → userId (Guid)
/// - ClaimTypes.Email          → email
/// - ClaimTypes.Role           → role (Admin/Waiter/Cashier)
/// → [Authorize(Roles = "Admin")] tự đọc ClaimTypes.Role để authorize
/// </summary>
public class AuthService(
    IUnitOfWork unitOfWork,
    IConfiguration configuration) : IAuthService
{
    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        // 1. Tìm staff theo email (case-insensitive)
        var staffList = await unitOfWork.Staff.FindAsync(
            s => s.Email.ToLower() == request.Email.ToLower());
        var staff = staffList.FirstOrDefault()
            ?? throw new UnauthorizedException("Email hoặc mật khẩu không đúng.");

        // 2. Kiểm tra account active
        if (!staff.IsActive)
            throw new UnauthorizedException("Tài khoản đã bị khóa. Liên hệ admin.");

        // 3. Verify password với BCrypt
        // BCrypt.Verify: compare plaintext với hash → an toàn, không lộ password
        if (!BCrypt.Net.BCrypt.Verify(request.Password, staff.PasswordHash))
            throw new UnauthorizedException("Email hoặc mật khẩu không đúng.");

        // 4. Generate JWT
        var token = GenerateJwtToken(staff);

        return new LoginResponse(
            AccessToken: token,
            ExpiresAt:   DateTime.UtcNow.AddHours(GetJwtExpiryHours()),
            FullName:    staff.FullName,
            Email:       staff.Email,
            Role:        staff.Role.ToString()
        );
    }

    public async Task<StaffDto> RegisterStaffAsync(RegisterStaffRequest request, CancellationToken cancellationToken = default)
    {
        // Kiểm tra email đã tồn tại chưa
        var existing = await unitOfWork.Staff.FindAsync(s => s.Email.ToLower() == request.Email.ToLower());
        if (existing.Any())
            throw new ConflictException($"Email '{request.Email}' đã được sử dụng.");

        var staff = new Staff
        {
            FullName     = request.FullName,
            Email        = request.Email.ToLower(),
            // BCrypt.HashPassword: tự gen salt và hash → mỗi lần hash khác nhau
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role         = request.Role,
            PhoneNumber  = request.PhoneNumber,
            IsActive     = true
        };

        await unitOfWork.Staff.AddAsync(staff);
        await unitOfWork.SaveChangesAsync();

        return MapToDto(staff);
    }

    public async Task<StaffDto> GetStaffByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var staff = await unitOfWork.Staff.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Staff), id);
        return MapToDto(staff);
    }

    public async Task<IEnumerable<StaffDto>> GetAllStaffAsync(CancellationToken cancellationToken = default)
    {
        var staffList = await unitOfWork.Staff.GetAllAsync();
        return staffList.Select(MapToDto);
    }

    public async Task<StaffDto> ToggleStaffStatusAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var staff = await unitOfWork.Staff.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Staff), id);

        staff.IsActive = !staff.IsActive;
        unitOfWork.Staff.Update(staff);
        await unitOfWork.SaveChangesAsync();

        return MapToDto(staff);
    }

    // ===== Private Helpers =====

    private string GenerateJwtToken(Staff staff)
    {
        var jwtSection = configuration.GetSection("Jwt");
        var secretKey  = jwtSection["SecretKey"]
            ?? throw new InvalidOperationException("JWT SecretKey không được cấu hình.");

        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Claims: dữ liệu được embed vào token (readable bởi client, signed bởi server)
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,   staff.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, staff.Email),
            new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()), // JWT ID (prevent replay)
            new Claim(ClaimTypes.NameIdentifier,     staff.Id.ToString()),
            new Claim(ClaimTypes.Email,              staff.Email),
            new Claim(ClaimTypes.Name,               staff.FullName),
            new Claim(ClaimTypes.Role,               staff.Role.ToString()),
        };

        var token = new JwtSecurityToken(
            issuer:   jwtSection["Issuer"] ?? "DineFlow",
            audience: jwtSection["Audience"] ?? "DineFlowClient",
            claims:   claims,
            expires:  DateTime.UtcNow.AddHours(GetJwtExpiryHours()),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private int GetJwtExpiryHours()
        => int.TryParse(configuration["Jwt:ExpiryHours"], out var hours) ? hours : 8;

    private static StaffDto MapToDto(Staff staff) => new(
        Id:          staff.Id,
        FullName:    staff.FullName,
        Email:       staff.Email,
        Role:        staff.Role.ToString(),
        PhoneNumber: staff.PhoneNumber,
        IsActive:    staff.IsActive,
        CreatedAt:   staff.CreatedAt
    );
}
