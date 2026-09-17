using FinanSmart.Api.Data.Context;
using FinanSmart.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanSmart.Api.Data.Seed;

public static class RoleSeed
{
    private static readonly string[] RoleNames = ["Admin", "Advisor", "Client"];

    public static async Task SeedAsync(FinanSmartDbContext context)
    {
        var now = DateTimeOffset.UtcNow;
        var existingRoleNames = await context.Roles
            .Where(role => RoleNames.Contains(role.Name))
            .Select(role => role.Name)
            .ToListAsync();

        var rolesToCreate = RoleNames
            .Except(existingRoleNames)
            .Select(name => new Role
            {
                Id = Guid.NewGuid(),
                Name = name,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            });

        context.Roles.AddRange(rolesToCreate);
        await context.SaveChangesAsync();
    }
}

