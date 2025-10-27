using Microsoft.Extensions.Configuration;

namespace OzoraSoft.Library.Security
{
    public interface IApiKeyValidator
    {
        bool IsValid(string apiKey);
    }

    public class ApiKeyValidator : IApiKeyValidator
    {
        private readonly IConfiguration _configuration;

        public ApiKeyValidator(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public bool IsValid(string userApiKey)
        {
            // Retrieve API key from configuration (e.g., appsettings.json)
            string storedApiKey = _configuration["ApiKey"]!;
            return !string.IsNullOrEmpty(userApiKey) && userApiKey == storedApiKey;
        }
    }
}
