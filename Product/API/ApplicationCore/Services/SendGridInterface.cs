using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.Models;
using UpDiddyLib.Dto.Marketing;
using UpDiddyLib.Helpers;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class SendGridInterface : BusinessVendorBase, ISendGridInterface
    {

        public SendGridInterface(UpDiddyDbContext context, IMapper mapper, Microsoft.Extensions.Configuration.IConfiguration configuration, ILogger sysLog, IHttpClientFactory httpClientFactory)
        {
            _db = context;
            _mapper = mapper;
            _apiBaseUri = configuration["SendEmail:ApiUrl"];
            _accessToken = configuration["SysEmail:ApiKey"];
            _syslog = sysLog;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        private HttpResponseMessage CreateContactList(SendGridListDto ListInfo)
        {
            HttpClient client = CreateWozPostClient();
            HttpRequestMessage WozRequest = CreateWozPostRequest(ApiAction, Content);
            HttpResponseMessage WozResponse = AsyncHelper.RunSync<HttpResponseMessage>(() => client.SendAsync(WozRequest));
            ResponseJson = AsyncHelper.RunSync<string>(() => WozResponse.Content.ReadAsStringAsync());
            return WozResponse;

        }


        #region Utility Functions

        private string GetCourseCodeFromSection(string SectionId)
        {
            string Rval = string.Empty;
            try
            {
                string ResponseJson = string.Empty;
                ExecuteWozGet($"sections/{SectionId}", ref ResponseJson);
                var ResponseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(ResponseJson);
                Rval = ResponseObject.courseCode;
            }
            catch { }
            return Rval;
        }


        private HttpResponseMessage ExecuteWozPost(string ApiAction, string Content, ref string ResponseJson)
        {
            HttpClient client = CreateWozPostClient();
            HttpRequestMessage WozRequest = CreateWozPostRequest(ApiAction, Content);
            HttpResponseMessage WozResponse = AsyncHelper.RunSync<HttpResponseMessage>(() => client.SendAsync(WozRequest));
            ResponseJson = AsyncHelper.RunSync<string>(() => WozResponse.Content.ReadAsStringAsync());
            return WozResponse;

        }

        private HttpResponseMessage ExecuteWozGet(string ApiAction, ref string ResponseJson)
        {

            HttpClient client = CreateWozGetClient();
            HttpRequestMessage WozRequest = CreateWozGetRequest(ApiAction);
            HttpResponseMessage WozResponse = AsyncHelper.RunSync<HttpResponseMessage>(() => client.SendAsync(WozRequest));
            ResponseJson = AsyncHelper.RunSync<string>(() => WozResponse.Content.ReadAsStringAsync());
            return WozResponse;
        }


        private HttpRequestMessage CreateWozPostRequest(string ApiAction, string Content)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, _apiBaseUri + ApiAction)
            {
                Content = new StringContent(Content)
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return request;
        }

        private HttpRequestMessage CreateWozGetRequest(string ApiAction)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, _apiBaseUri + ApiAction);
            return request;

        }

        private HttpClient CreateWozPostClient()
        {
            HttpClient client = _httpClientFactory.CreateClient(Constants.HttpPostClientName);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            return client;

        }

        private HttpClient CreateWozGetClient()
        {
            HttpClient client = _httpClientFactory.CreateClient(Constants.HttpGetClientName);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            return client;

        }

        private MessageTransactionResponse CreateResponse(string ResponseJson, string Info, string Data, TransactionState State)
        {
            MessageTransactionResponse RVal = new MessageTransactionResponse()
            {
                ResponseJson = ResponseJson,
                InformationalMessage = Info,
                Data = Data,
                State = State
            };

            string RValJson = Newtonsoft.Json.JsonConvert.SerializeObject(RVal);

            if (_translog.WozTransactionLogId > 0) _translog.WozTransactionLogId = 0;
            _translog.ResponseJson = RValJson;
            _translog.ModifyDate = DateTime.UtcNow;
            _translog.CreateDate = DateTime.UtcNow;
            _translog.CreateGuid = Guid.Empty;
            _translog.ModifyGuid = Guid.Empty;

            _db.WozTransactionLog.Add(_translog);
            _db.SaveChanges();

            return RVal;
        }

        public static DateTime FromWozTime(long wozTime)
        {
            return epoch.AddMilliseconds(wozTime);
        }

        public static long ToWozTime(DateTime dateTime)
        {
            return (long)(dateTime - epoch).TotalMilliseconds;
        }

        private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    }

    #endregion




}
}
