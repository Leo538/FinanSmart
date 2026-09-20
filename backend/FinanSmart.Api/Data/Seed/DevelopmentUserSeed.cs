using FinanSmart.Api.Data.Context;
using FinanSmart.Api.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FinanSmart.Api.Data.Seed;

public static class DevelopmentUserSeed
{
    private static readonly (string FirstName, string LastName, string Email, string Role)[] Users = [("Local", "Administrator", "admin@finansmart.local", "Admin"), ("Local", "Advisor", "advisor@finansmart.local", "Advisor"), ("Local", "Client", "client@finansmart.local", "Client")];

    public static async Task SeedAsync(FinanSmartDbContext context, PasswordHasher<User> passwordHasher)
    {
        var roleByName = await context.Roles.Where(x => Users.Select(u => u.Role).Contains(x.Name)).ToDictionaryAsync(x => x.Name);
        var emails = Users.Select(x => x.Email).ToArray();
        var existingEmails = await context.Users.Where(x => emails.Contains(x.Email)).Select(x => x.Email).ToListAsync();
        var now = DateTimeOffset.UtcNow;
        foreach (var item in Users.Where(x => !existingEmails.Contains(x.Email)))
        {
            var user = new User { Id = Guid.NewGuid(), FirstName = item.FirstName, LastName = item.LastName, Email = item.Email, IsActive = true, CreatedAt = now, UpdatedAt = now };
            user.PasswordHash = passwordHasher.HashPassword(user, "FinanSmart123!");
            context.Users.Add(user);
            context.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = roleByName[item.Role].Id });
        }
        await context.SaveChangesAsync();
    }
}
