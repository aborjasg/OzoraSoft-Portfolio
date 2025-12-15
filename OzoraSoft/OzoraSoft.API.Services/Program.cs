using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;
using OzoraSoft.DataSources;
using OzoraSoft.Library.Enums.Shared;
using OzoraSoft.Library.Security;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add Authentication
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

builder.Services.AddAuthorization();

// DB Contexts
builder.Services.AddDbContext<OzoraSoft_InfoSecControls_DBContext>(options =>
    options.UseMySQL(
            UtilsForMessages.Decompress(builder.Configuration.GetConnectionString("OzoraSoft_InfoSecControls_Connection")!),
            mySqlOptions => mySqlOptions.EnableRetryOnFailure()
        ));

builder.Services.AddDbContext<OzoraSoft_Transit_DBContext>(options =>
    options.UseMySQL(
            UtilsForMessages.Decompress(builder.Configuration.GetConnectionString("OzoraSoft_Transit_Connection")!),
            mySqlOptions => mySqlOptions.EnableRetryOnFailure()
        ));

builder.Services.AddDbContext<OzoraSoft_Shared_DBContext>(options =>
    options.UseMySQL(
            UtilsForMessages.Decompress(builder.Configuration.GetConnectionString("OzoraSoft_Shared_Connection")!),
            mySqlOptions => mySqlOptions.EnableRetryOnFailure()
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
app.MapGet("/secure", [Authorize] () => "Secure OK");

app.Run();
