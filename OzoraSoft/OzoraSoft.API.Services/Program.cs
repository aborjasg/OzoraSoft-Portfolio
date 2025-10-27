using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Orestes.SharedLibrary;
using OzoraSoft.DataSources;
using OzoraSoft.Library.Security;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add Authorization
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
       .AddJwtBearer(options =>
       {
           options.TokenValidationParameters = new TokenValidationParameters
           {
               ValidateIssuerSigningKey = true,
               IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]!)),
               ValidateIssuer = true,
               ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
               ValidateAudience = true,
               ValidAudience = builder.Configuration["JwtSettings:Audience"],
               ValidateLifetime = true,
               ClockSkew = TimeSpan.Zero // optional, removes default 5 min clock skew
           };
       });
//builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.AddSingleton<IJwtSettings, JwtSettings>(e => builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()!);

builder.Services.AddDbContext<OzoraSoftDBContext>(options =>
    options.UseMySql(
            UtilsForMessages.Decompress(builder.Configuration.GetConnectionString("OzoraSoftInfoSecControlsConnection")!),
            ServerVersion.AutoDetect(UtilsForMessages.Decompress(builder.Configuration.GetConnectionString("OzoraSoftInfoSecControlsConnection")!))
        ));

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
