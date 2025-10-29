using Org.BouncyCastle.Asn1.Ocsp;
using OzoraSoft.API.Utils.Models;
using OzoraSoft.DataSources.InfoSecControls;
using OzoraSoft.Library.Security;
using OzoraSoft.Library.Security.Services;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using static System.Net.WebRequestMethods;

namespace OzoraSoft.Web
{    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="httpClient"></param>
    public class OzoraSoft_API_Services_Client(HttpClient httpClient)
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="accessToken"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<SystemParameter[]> SystemParameters_GetList(int groupId, string accessToken, CancellationToken cancellationToken = default)
        {
            //TODO: Add pagination object for all the lists from DB. Take as reference previous project implementation
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var result = new SystemParameter[] { };
            using var response = await httpClient.GetAsync($"{ApiServices.API_INFOSECCONTROLS_SYSTEMPARAMETERS}/{groupId}", cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                result = await response.Content.ReadFromJsonAsync<SystemParameter[]>(cancellationToken: cancellationToken);
            }
            return result!;
        }

        public async Task<OrganizationPolicy[]> OrganizationPolicies_GetList(string accessToken, CancellationToken cancellationToken = default)
        {
            //TODO: Add pagination object for all the lists from DB. Take as reference previous project implementation
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var result = new OrganizationPolicy[] { };
            using var response = await httpClient.GetAsync($"{ApiServices.API_INFOSECCONTROLS_ORGANIZATIONPOLICIES}", cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                result = await response.Content.ReadFromJsonAsync<OrganizationPolicy[]>(cancellationToken: cancellationToken);
            }
            return result!;
        }
    }
}
