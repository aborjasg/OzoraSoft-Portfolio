using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OzoraSoft.Library.Security.Services
{
    public static class ApiServices
    {
        public const string API_AUTHENTICATION_LOGIN = "/api/Authentication/login";
        public const string API_INFOSECCONTROLS_SYSTEMPARAMETERS = "/api/SystemParameters";
        public const string API_INFOSECCONTROLS_ORGANIZATIONPOLICIES = "/api/OrganizationPolicies";
        public const string API_MESSAGING_COMPRESS = "/api/Messaging/compress";
        public const string API_MESSAGING_DECOMPRESS = "/api/Messaging/decompress";
        public const string API_MESSAGING_ENCRYPT = "/api/Messaging/encrypt";
        public const string API_MESSAGING_DECRYPT = "/api/Messaging/decrypt";
        public const string API_EVENTLOG = "/api/EventLogs";
        public const string API_EVENTLOG_LIST = "/api/EventLogs/list";
        public const string API_CHATHUB_ENDPOINT = "/chathub";
        public const string API_ERROR_ENDPOINT = "/Error";
        public const string API_TRANSIT_VIDEOCAPTURE = "/api/VideoCaptures";
    }
}
