using FinanSmart.Api.Configurations;
using FinanSmart.Api.Data.Context;
using FinanSmart.Api.Data.Seed;
using FinanSmart.Api.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("Connection string 'DefaultConnection' is required. Configure it locally or via ConnectionStrings__DefaultConnection.");

builder.Services.AddDbContext<FinanSmartDbContext>(options => options.UseNpgsql(connectionString));
var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("JWT configuration was not found.");

if (string.IsNullOrWhiteSpace(jwtOptions.Issuer)
    || string.IsNullOrWhiteSpace(jwtOptions.Audience)
    || string.IsNullOrWhiteSpace(jwtOptions.Key)
    || jwtOptions.AccessTokenMinutes <= 0
    || jwtOptions.RefreshTokenDays <= 0)
{
    throw new InvalidOperationException("JWT configuration is incomplete.");
}

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtOptions.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtOptions.Audience,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
        ClockSkew = TimeSpan.Zero
    });
builder.Services.AddAuthorization();
builder.Services.AddHttpClient();
builder.Services.AddCors(options => options.AddPolicy("Frontend", policy => policy
    .WithOrigins("http://localhost:4200")
    .AllowAnyHeader()
    .AllowAnyMethod()));
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
// Only institution logos are public. Application documents and identity files stay private.
var publicLogoDirectory = Path.Combine(app.Environment.ContentRootPath, "Storage", "institution-logos");
Directory.CreateDirectory(publicLogoDirectory);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(publicLogoDirectory),
    RequestPath = "/storage/institution-logos"
});
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
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
            var passwordHasher = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.PasswordHasher<FinanSmart.Api.Entities.User>>();
            if (app.Environment.IsDevelopment())
                await DevelopmentUserSeed.SeedAsync(dbContext, passwordHasher);
        }
    }
    catch
    {
        // The database health endpoint reports unavailable connections.
    }
}

app.Run();
