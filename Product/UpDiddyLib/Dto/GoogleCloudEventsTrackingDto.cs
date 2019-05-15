using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Text;
using UpDiddyLib.Shared.GoogleJobs;

namespace UpDiddyLib.Dto
{
    public class GoogleCloudEventsTrackingDto
    {
        public const string REQUEST_ID_KEY = "r";
        public const string PARENT_EVENT_ID_KEY = "p";

        public string RequestId { get; set; }
        public string ParentClientEventId { get; set; }
        public string ClientEventId { get; set; }
        public ClientEventType Type { get; set; }
        public static GoogleCloudEventsTrackingDto Build(IQueryCollection query, ClientEventType eventType)
        {
            GoogleCloudEventsTrackingDto dto = new GoogleCloudEventsTrackingDto()
            {
                RequestId = query[REQUEST_ID_KEY],
                ParentClientEventId = query[PARENT_EVENT_ID_KEY],
                Type = eventType
            };
            return dto;
        }

        public static GoogleCloudEventsTrackingDto Build(string url, ClientEventType eventType)
        {
            string[] urlParts = url.Split("?");
            if (urlParts.Length <= 1)
                return null;

            Dictionary<string, StringValues> queryParams = QueryHelpers.ParseQuery(urlParts[1]);

            return new GoogleCloudEventsTrackingDto()
            {
                RequestId = queryParams[REQUEST_ID_KEY],
                ParentClientEventId = queryParams[PARENT_EVENT_ID_KEY],
                Type = eventType
            };
        }

        public static string AddParametersToUrl(string url, string requestId, string parentClientEventId)
        {
            string newUrl = url;
            if(requestId != null)
                newUrl = QueryHelpers.AddQueryString(url, REQUEST_ID_KEY, requestId);
            if(parentClientEventId != null)
                newUrl = QueryHelpers.AddQueryString(newUrl, PARENT_EVENT_ID_KEY, parentClientEventId);
            return newUrl;
        }

        public string AddParamsToUrl(string url)
        {
            return AddParametersToUrl(url, RequestId, ParentClientEventId);
        }
    }
}