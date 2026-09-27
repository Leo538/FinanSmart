using FinanSmart.Api.Modules.Auth.DTOs;

namespace FinanSmart.Api.Modules.Auth.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);
    Task<bool> RegisterClientAsync(RegisterClientRequestDto request);
    Task<LoginResponseDto?> RefreshAsync(string refreshToken);
    Task LogoutAsync(Guid userId);
    Task<CurrentUserDto?> GetCurrentUserAsync(Guid userId);
}
