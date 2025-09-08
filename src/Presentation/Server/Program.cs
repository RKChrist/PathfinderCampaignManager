using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PathfinderCampaignManager.Infrastructure.Persistence;
using PathfinderCampaignManager.Infrastructure.Services;
using PathfinderCampaignManager.Presentation.Server.Hubs;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Add SignalR
builder.Services.AddSignalR();

// Add Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("PathfinderCampaignManagerDb"));

// Add HttpClient for Discord API calls
builder.Services.AddHttpClient();

// Add Authentication Services
builder.Services.AddScoped<PathfinderCampaignManager.Application.Abstractions.IJwtAuthenticationService, PathfinderCampaignManager.Infrastructure.Services.JwtAuthenticationService>();
builder.Services.AddScoped<PathfinderCampaignManager.Application.Abstractions.IPasswordService, PathfinderCampaignManager.Infrastructure.Services.PasswordService>();
builder.Services.AddScoped<PathfinderCampaignManager.Application.Abstractions.IUserRepository, PathfinderCampaignManager.Infrastructure.Repositories.UserRepository>();
builder.Services.AddScoped<PathfinderCampaignManager.Application.Services.AuthenticationService>();

// Add Pathfinder Data Services
builder.Services.AddScoped<PathfinderCampaignManager.Domain.Interfaces.IPathfinderDataRepository, PathfinderCampaignManager.Infrastructure.Data.PathfinderDataRepository>();
builder.Services.AddScoped<PathfinderCampaignManager.Domain.Interfaces.IRulesRepository, PathfinderCampaignManager.Infrastructure.Data.RulesRepository>();
builder.Services.AddScoped<PathfinderCampaignManager.Domain.Interfaces.IArchetypeRepository, PathfinderCampaignManager.Infrastructure.Data.ArchetypeRepository>();
builder.Services.AddScoped<PathfinderCampaignManager.Domain.Interfaces.IArchetypeService, PathfinderCampaignManager.Infrastructure.Services.ArchetypeService>();

// Add Validation Services
builder.Services.AddScoped<PathfinderCampaignManager.Domain.Validation.IValidationService, PathfinderCampaignManager.Infrastructure.Validation.ValidationService>();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = builder.Configuration["Jwt:SecretKey"] ?? "your-very-secure-secret-key-that-is-at-least-256-bits-long";
var issuer = builder.Configuration["Jwt:Issuer"] ?? "PathfinderCampaignManager";
var audience = builder.Configuration["Jwt:Audience"] ?? "PathfinderCampaignManager.Client";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ValidateIssuer = true,
        ValidIssuer = issuer,
        ValidateAudience = true,
        ValidAudience = audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
    
    // Configure JWT for SignalR
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && (path.StartsWithSegments("/combathub") || path.StartsWithSegments("/campaignhub")))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy.WithOrigins("https://localhost:7082", "http://localhost:7082")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .SetIsOriginAllowedToAllowWildcardSubdomains();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseCors("AllowBlazorClient");
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();
app.MapHub<CombatHub>("/combathub");
app.MapHub<CampaignHub>("/campaignhub");
app.MapFallbackToFile("index.html");

app.Run();
