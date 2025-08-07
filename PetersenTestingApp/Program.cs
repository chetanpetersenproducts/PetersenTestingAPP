using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Identity.Web;
using PetersenTestingApp.Components;
using PetersenTestingAppLibrary.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents().AddMicrosoftIdentityConsentHandler();

builder.Services.AddSingleton<PetersenTestingAppLibrary.Classes.Utils>(new PetersenTestingAppLibrary.Classes.Utils());
builder.Services.AddScoped(ServiceProvider => new UserService(ServiceProvider.GetRequiredService<PetersenTestingAppLibrary.Classes.Utils>()));
builder.Services.AddScoped(ServiceProvider => new BackendService(ServiceProvider.GetRequiredService<PetersenTestingAppLibrary.Classes.Utils>()));
builder.Services.AddScoped(ServiceProvider => new TestingService(ServiceProvider.GetRequiredService<PetersenTestingAppLibrary.Classes.Utils>()));
builder.Services.AddScoped(ServiceProvider => new DashboardBackendService(ServiceProvider.GetRequiredService<PetersenTestingAppLibrary.Classes.Utils>()));

builder.Services.TryAddEnumerable(
    ServiceDescriptor.Scoped<CircuitHandler, UserCircuitHandler>());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>().RequireAuthorization(
    new AuthorizeAttribute
    {
        AuthenticationSchemes = OpenIdConnectDefaults.AuthenticationScheme
    })
    .AddInteractiveServerRenderMode();

app.Run();
