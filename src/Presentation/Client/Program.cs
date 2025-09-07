using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PathfinderCampaignManager.Presentation.Client;
using PathfinderCampaignManager.Presentation.Client.Services;
using PathfinderCampaignManager.Presentation.Client.Components.HoverCards;
using Fluxor;
using System.Reflection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Add SignalR Services
builder.Services.AddScoped<CombatSignalRService>();
builder.Services.AddScoped<CampaignSignalRService>();

// Add API-based Data Services
builder.Services.AddScoped<PathfinderApiService>();
builder.Services.AddScoped<RulesApiService>();

// Add Hover Card Service
builder.Services.AddScoped<PathfinderCampaignManager.Presentation.Client.Services.HoverCardService>();
builder.Services.AddScoped<IHoverCardService>(provider => provider.GetRequiredService<PathfinderCampaignManager.Presentation.Client.Services.HoverCardService>());

// Add Validation Service
builder.Services.AddScoped<ValidationClientService>();

// Add Authentication Service
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

// Add Fluxor
builder.Services.AddFluxor(options =>
{
    options.ScanAssemblies(Assembly.GetExecutingAssembly());
});

await builder.Build().RunAsync();
