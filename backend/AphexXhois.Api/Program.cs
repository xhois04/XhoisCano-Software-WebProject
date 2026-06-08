using System.Text;
using AphexXhois.Api.Data;
using AphexXhois.Api.Models;
using AphexXhois.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("Jwt:Secret is not configured.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? ["http://localhost:5000", "https://localhost:7000"];
        policy.WithOrigins(origins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddHttpClient();
builder.Services.AddScoped<JwtService>();
builder.Services.AddSingleton<SpotifyService>();

var app = builder.Build();

// Return JSON error details in development so we can see what's crashing
app.UseExceptionHandler(errApp =>
{
    errApp.Run(async ctx =>
    {
        ctx.Response.ContentType = "application/json";
        var ex = ctx.Features.Get<IExceptionHandlerFeature>()?.Error;
        var msg = app.Environment.IsDevelopment()
            ? ex?.ToString()
            : ex?.Message ?? "Internal server error";
        ctx.Response.StatusCode = 500;
        await ctx.Response.WriteAsync(
            System.Text.Json.JsonSerializer.Serialize(new { message = msg }));
    });
});

app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();

// Simple health check — open http://localhost:5102/api/health in browser to verify API is up
app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));

app.MapControllers();

// Seed admin user on first run
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    if (!db.Users.Any(u => u.Role == UserRole.ADMIN))
    {
        db.Users.Add(new User
        {
            Username = "admin",
            Email = "admin@musictwins.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            Role = UserRole.ADMIN
        });
        db.SaveChanges();
    }
}

app.Run();