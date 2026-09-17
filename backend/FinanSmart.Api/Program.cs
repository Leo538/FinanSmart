using FinanSmart.Api.Data.Context;
using FinanSmart.Api.Data.Seed;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

builder.Services.AddDbContext<FinanSmartDbContext>(options => options.UseNpgsql(connectionString));

var app = builder.Build();

app.MapGet("/api/health", () => Results.Ok(new
{
    status = "ok",
    application = "FinanSmart.Api"
}));

app.MapGet("/api/health/database", async (FinanSmartDbContext dbContext) =>
{
    try
    {
        var canConnect = await dbContext.Database.CanConnectAsync();

        return canConnect
            ? Results.Ok(new { status = "ok", database = "connected" })
            : Results.Json(new { status = "error", database = "unavailable" }, statusCode: StatusCodes.Status503ServiceUnavailable);
    }
    catch
    {
        return Results.Json(new { status = "error", database = "unavailable" }, statusCode: StatusCodes.Status503ServiceUnavailable);
    }
});

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<FinanSmartDbContext>();
    try
    {
        if (await dbContext.Database.CanConnectAsync())
        {
            await RoleSeed.SeedAsync(dbContext);
        }
    }
    catch
    {
        // The database health endpoint reports unavailable connections.
    }
}

app.Run();
