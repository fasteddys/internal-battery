using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddy.Helpers
{
    static public class Constants
    {
        static public string  SubsriberSessionKey = "Subscriber";
        static public readonly string EMPTY_STRING = "";
        static public readonly string HttpGetClientName = "HttpGetClient";
        static public readonly string HttpPostClientName = "HttpPostClient";
        static public readonly string HttpPutClientName = "HttpPutClient";
        static public readonly string HttpDeleteClientName = "HttpDeleteClient";
        static public readonly string PollyStringCacheName = "PollyStringCacheName";
        static public readonly int PollyStringCacheTimeInMinutes = 5;
        static public readonly string SysLogLogInformationTrue = "true";
    }
}
