using FinanSmart.Api.Configurations;
using FinanSmart.Api.Data.Context;
using FinanSmart.Api.Data.Seed;
using FinanSmart.Api.Middlewares;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

builder.Services.AddDbContext<FinanSmartDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddApplicationServices();
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.MapOpenApi();
app.MapControllers();

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
