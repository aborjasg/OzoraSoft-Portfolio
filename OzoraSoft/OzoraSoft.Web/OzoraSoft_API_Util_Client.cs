using Microsoft.Extensions.Options;
using Org.BouncyCastle.Asn1.Ocsp;
using OzoraSoft.API.Utils.Models;
using OzoraSoft.Library.Security;
using OzoraSoft.Library.Security.Services;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using static System.Net.WebRequestMethods;

namespace OzoraSoft.Web
{
    public class OzoraSoft_API_Util_Client(HttpClient httpClient, LoginModel loginModel)
    {
        private string _username = loginModel.Username;
        private string _password = loginModel.Password;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<TokenResponse> AccessToken_Get(CancellationToken cancellationToken = default)
        {
            LoginModel model = new() { Username = _username , Password = _password };
            var result = new TokenResponse();

            using var response = await httpClient.PostAsJsonAsync(ApiServices.API_AUTHENTICATION_LOGIN, model, cancellationToken); ;
            if (response.IsSuccessStatusCode)
            {
                // if API returns JSON body, deserialize it directly:
                result = await response.Content.ReadFromJsonAsync<TokenResponse>();
            }
            return result!;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uncompressedText"></param>
        /// <param name="accessToken"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<string> Messaging_CompressText(string uncompressedText, string accessToken, CancellationToken cancellationToken = default)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var result = new Message() { Input = uncompressedText };
            using var response = await httpClient.PostAsJsonAsync(ApiServices.API_MESSAGING_COMPRESS, result, cancellationToken); ;

            if (response.IsSuccessStatusCode)
            {
                // if API returns JSON body, deserialize it directly:
                result = await response.Content.ReadFromJsonAsync<Message>();
            }
            return result!.Output;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="compressedText"></param>
        /// <param name="accessToken"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<string> Messaging_UncompressText(string compressedText, string accessToken, CancellationToken cancellationToken = default)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var result = new Message() { Input = compressedText };
            using var response = await httpClient.PostAsJsonAsync(ApiServices.API_MESSAGING_DECOMPRESS, result, cancellationToken); ;

            if (response.IsSuccessStatusCode)
            {
                // if API returns JSON body, deserialize it directly:
                result = await response.Content.ReadFromJsonAsync<Message>();
            }
            return result!.Output;
        }

        public async Task<string> Messaging_EncryptText(string input, string accessToken, CancellationToken cancellationToken = default)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var result = new Message() { Input = input };
            using var response = await httpClient.PostAsJsonAsync(ApiServices.API_MESSAGING_ENCRYPT, result, cancellationToken); ;

            if (response.IsSuccessStatusCode)
            {
                // if API returns JSON body, deserialize it directly:
                result = await response.Content.ReadFromJsonAsync<Message>();
            }
            return result!.Output;
        }

        public async Task<string> Messaging_DecryptText(string input, string accessToken, CancellationToken cancellationToken = default)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var result = new Message() { Input = input };
            using var response = await httpClient.PostAsJsonAsync(ApiServices.API_MESSAGING_DECRYPT, result, cancellationToken); ;

            if (response.IsSuccessStatusCode)
            {
                // if API returns JSON body, deserialize it directly:
                result = await response.Content.ReadFromJsonAsync<Message>();
            }
            return result!.Output;
        }

    }
}
