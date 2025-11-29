using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OzoraSoft.Library.Enums.Shared
{
    public enum enumEventLogProject
    {
        OzoraSoft_APIServices = 1,
        OzoraSoft_APIUtils = 2,
        OzoraSoft_Console = 3,
        OzoraSoft_Tests = 4,
        OzoraSoft_AppHost = 5,
        OzoraSoft_ServiceDefaults = 6,
        OzoraSoft_Web = 7,
        OzoraSoft_Library_Enums = 8,
        OzoraSoft_Library_Messaging = 9,
        OzoraSoft_Library_PictureMaker = 10,
        OzoraSoft_Library_Security = 11
    }

    public enum enumEventLogModule
    {
        Shared = 1,
        Security = 2,
        InfoSecControls = 3,
        PictureMaker = 4
    }

    public enum enumEventLogController
    {
        Utils_Authentication = 1,
        Utils_Messaging = 2,
        Shared_EventLogs = 3,
        InfoSecControls_SystemParameters = 4,
        InfoSecControls_OrganizationPolicies = 5
    }

    public enum enumEventLogAction
    {
        Create = 1,
        Read = 2,
        Update = 3,
        Delete = 4,
        List = 5,
        Execute = 6,
        Import = 7,
        Export = 8
    }
}
