using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;
using OzoraSoft.API.Services.Models;
using OzoraSoft.API.Utils.Models;
using OzoraSoft.DataSources.InfoSecControls;
using OzoraSoft.DataSources.Shared;
using OzoraSoft.DataSources.Transit;
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<EventLog[]> EventLogs_GetList(EventLog_Filter filter, string accessToken, CancellationToken cancellationToken = default)
        {
            //TODO: Add pagination object for all the lists from DB. Take as reference previous project implementation
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var result = new EventLog[] { };
            using var response = await httpClient.PostAsJsonAsync($"{ApiServices.API_EVENTLOG_LIST}", filter, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                result = await response.Content.ReadFromJsonAsync<EventLog[]>(cancellationToken: cancellationToken);
            }
            return result!.OrderByDescending(x => x.process_datetime).ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="record"></param>
        /// <param name="accessToken"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<int> EventLogs_Add(EventLog record, string accessToken, CancellationToken cancellationToken = default)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            int result = 0;

            using var response = await httpClient.PostAsJsonAsync($"{ApiServices.API_EVENTLOG}", record, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                result = await response.Content.ReadFromJsonAsync<int>(cancellationToken: cancellationToken);
            }
            return result!;
        }

        public async Task<int> VideoCaptures_Add(VideoCapture record, string accessToken, CancellationToken cancellationToken = default)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            int result = 0;

            using var response = await httpClient.PostAsJsonAsync($"{ApiServices.API_TRANSIT_VIDEOCAPTURE}", record, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var obj = await response.Content.ReadFromJsonAsync<VideoCapture>(cancellationToken: cancellationToken);
                result = obj!.id;
            }
            return result!;
        }

        public async Task<VideoCapture> VideoCaptures_Get(int recordId, string accessToken, CancellationToken cancellationToken = default)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            VideoCapture result = new VideoCapture() { };

            using var response = await httpClient.GetAsync($"{ApiServices.API_TRANSIT_VIDEOCAPTURE}/{recordId}", cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var obj = await response.Content.ReadFromJsonAsync<VideoCapture>(cancellationToken: cancellationToken);
                if (obj != null) result = obj!;
            }
            return result!;
        }
    }
}
