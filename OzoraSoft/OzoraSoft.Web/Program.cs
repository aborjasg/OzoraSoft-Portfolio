using OzoraSoft.Library.Security;
using OzoraSoft.Web;
using OzoraSoft.Web.Components;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddControllers();

builder.Services.AddOutputCache();

builder.Services.AddHttpClient<OzoraSoft_API_Util_Client>(client =>
    {
        client.BaseAddress = new("https+http://ozorasoft-api-utils");
    });

builder.Services.AddHttpClient<OzoraSoft_API_Services_Client>(client =>
{
    client.BaseAddress = new("https+http://ozorasoft-api-services");
});

var section = builder.Configuration.GetSection("LoginModel");
var loginModel = section.Get<LoginModel>();
builder.Services.AddSingleton(loginModel!);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseHttpsRedirection();

app.UseAntiforgery();

app.UseOutputCache();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.Run();
