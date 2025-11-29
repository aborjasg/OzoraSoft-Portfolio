using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using OzoraSoft.Library.Messaging.Hubs;
using OzoraSoft.Library.Messaging.UI_Components;
using OzoraSoft.Library.Security;
using OzoraSoft.Library.Security.Services;
using OzoraSoft.Web;
using OzoraSoft.Web.Components;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddControllers();

builder.Services.AddOutputCache();

// API Services Clients
builder.Services.AddHttpClient<OzoraSoft_API_Util_Client>(client =>
    {
        client.BaseAddress = new("https+http://ozorasoft-api-utils");
    });

builder.Services.AddHttpClient<OzoraSoft_API_Services_Client>(client =>
{
    client.BaseAddress = new("https+http://ozorasoft-api-services");
});

// SignalR
builder.Services.AddSignalR();

builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        ["application/octet-stream"]);
});

// Login Model Configuration 
var section = builder.Configuration.GetSection("LoginModel");
var loginModel = section.Get<LoginModel>();
builder.Services.AddSingleton(loginModel!);

// UI Components
builder.Services.AddSingleton<ToastService>();

builder.Services.AddAuthorization();
builder.Services.AddControllersWithViews().AddMicrosoftIdentityUI();


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(ApiServices.API_ERROR_ENDPOINT, createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseResponseCompression();

app.UseHttpsRedirection();

app.UseAntiforgery();

app.UseOutputCache();

app.MapStaticAssets();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();
app.MapHub<ChatHub>(ApiServices.API_CHATHUB_ENDPOINT);

app.Run();
