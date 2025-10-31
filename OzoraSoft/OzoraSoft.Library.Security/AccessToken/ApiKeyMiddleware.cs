using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace OzoraSoft.Library.Security
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;

        public ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Headers.TryGetValue("x-api-key", out var extractedApiKey))
            {
                context.Response.StatusCode = 401; // Unauthorized
                return;
            }

            string storedApiKey = _configuration["ApiKey"]!; // Retrieve from configuration

            if (!extractedApiKey.Equals(storedApiKey))
            {
                context.Response.StatusCode = 401; // Unauthorized
                return;
            }

            await _next(context);
        }
    }
}
