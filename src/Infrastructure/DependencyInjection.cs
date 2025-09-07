using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PathfinderCampaignManager.Application.Abstractions;
using PathfinderCampaignManager.Domain.Interfaces;
using PathfinderCampaignManager.Infrastructure.Persistence;
using PathfinderCampaignManager.Infrastructure.Services;

namespace PathfinderCampaignManager.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection") ?? "Server=(localdb)\\mssqllocaldb;Database=PathfinderCampaignManager;Trusted_Connection=true;",
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        // Unit of Work and Repositories
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Application Services
        services.AddScoped<IDateTimeProvider, DateTimeProvider>();

        return services;
    }
}