using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FinanSmart.Api.Configurations;
using FinanSmart.Api.Data.Context;
using FinanSmart.Api.Entities;
using FinanSmart.Api.Modules.Auth.DTOs;
using FinanSmart.Api.Modules.Auth.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FinanSmart.Api.Modules.Auth.Services;

public class AuthService(FinanSmartDbContext dbContext, PasswordHasher<User> passwordHasher, IOptions<JwtOptions> jwtOptions) : IAuthService
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password)) return null;
        var email = request.Email.Trim().ToUpperInvariant();
        var user = await dbContext.Users.Include(x => x.UserRoles).ThenInclude(x => x.Role).SingleOrDefaultAsync(x => x.Email.ToUpper() == email);
        if (user is null || !user.IsActive) return null;
        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed) return null;
        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
            user.UpdatedAt = DateTimeOffset.UtcNow;
        }
        return await CreateTokenResponseAsync(user);
    }

    public async Task<bool> RegisterClientAsync(RegisterClientRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName) || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password)) throw new ArgumentException("Name, email and password are required.");
        var email = request.Email.Trim();
        if (!(new System.ComponentModel.DataAnnotations.EmailAddressAttribute()).IsValid(email)) throw new ArgumentException("A valid email is required.");
        if (await dbContext.Users.AnyAsync(user => user.Email.ToUpper() == email.ToUpper())) return false;
        var role = await dbContext.Roles.SingleAsync(role => role.Name == "Client");
        var now = DateTimeOffset.UtcNow;
        var user = new User { Id = Guid.NewGuid(), FirstName = request.FirstName.Trim(), LastName = request.LastName.Trim(), Email = email, IsActive = true, CreatedAt = now, UpdatedAt = now };
        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
        dbContext.Users.Add(user);
        dbContext.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });
        await dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<LoginResponseDto?> RefreshAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken)) return null;
        var stored = await dbContext.RefreshTokens.Include(x => x.User).ThenInclude(x => x.UserRoles).ThenInclude(x => x.Role).SingleOrDefaultAsync(x => x.TokenHash == HashRefreshToken(refreshToken));
        var now = DateTimeOffset.UtcNow;
        if (stored is null || stored.RevokedAt is not null || stored.ExpiresAt <= now || !stored.User.IsActive) return null;
        stored.RevokedAt = now;
        return await CreateTokenResponseAsync(stored.User, stored);
    }

    public async Task LogoutAsync(Guid userId)
    {
        var now = DateTimeOffset.UtcNow;
        var tokens = await dbContext.RefreshTokens.Where(x => x.UserId == userId && x.RevokedAt == null && x.ExpiresAt > now).ToListAsync();
        foreach (var token in tokens) token.RevokedAt = now;
        if (tokens.Count > 0) await dbContext.SaveChangesAsync();
    }

    public async Task<CurrentUserDto?> GetCurrentUserAsync(Guid userId)
    {
        var user = await dbContext.Users.AsNoTracking().Include(x => x.UserRoles).ThenInclude(x => x.Role).SingleOrDefaultAsync(x => x.Id == userId && x.IsActive);
        return user is null ? null : MapCurrentUser(user);
    }

    private async Task<LoginResponseDto> CreateTokenResponseAsync(User user, RefreshToken? replacedToken = null)
    {
        var now = DateTimeOffset.UtcNow;
        var accessExpiresAt = now.AddMinutes(_jwtOptions.AccessTokenMinutes);
        var refreshExpiresAt = now.AddDays(_jwtOptions.RefreshTokenDays);
        var refreshValue = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(64));
        var refreshToken = new RefreshToken { Id = Guid.NewGuid(), UserId = user.Id, TokenHash = HashRefreshToken(refreshValue), CreatedAt = now, ExpiresAt = refreshExpiresAt };
        dbContext.RefreshTokens.Add(refreshToken);
        if (replacedToken is not null) replacedToken.ReplacedByTokenId = refreshToken.Id;
        await dbContext.SaveChangesAsync();
        return new LoginResponseDto { AccessToken = CreateAccessToken(user, accessExpiresAt), AccessTokenExpiresAt = accessExpiresAt, RefreshToken = refreshValue, RefreshTokenExpiresAt = refreshExpiresAt, User = MapCurrentUser(user) };
    }

    private string CreateAccessToken(User user, DateTimeOffset expiresAt)
    {
        var claims = new List<Claim> { new(JwtRegisteredClaimNames.Sub, user.Id.ToString()), new(ClaimTypes.NameIdentifier, user.Id.ToString()), new(JwtRegisteredClaimNames.Email, user.Email), new(ClaimTypes.Email, user.Email) };
        claims.AddRange(user.UserRoles.Where(x => x.Role.IsActive).Select(x => new Claim(ClaimTypes.Role, x.Role.Name)));
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
        var token = new JwtSecurityToken(_jwtOptions.Issuer, _jwtOptions.Audience, claims, DateTime.UtcNow, expiresAt.UtcDateTime, new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static CurrentUserDto MapCurrentUser(User user) => new() { Id = user.Id, FirstName = user.FirstName, LastName = user.LastName, Email = user.Email, Roles = user.UserRoles.Where(x => x.Role.IsActive).Select(x => x.Role.Name).OrderBy(x => x).ToArray() };
    private static string HashRefreshToken(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
