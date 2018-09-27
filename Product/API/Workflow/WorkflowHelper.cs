using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using UpDiddyLib.Dto;
using UpDiddyLib.MessageQueue;

namespace UpDiddyApi.Workflow
{
    public class WorkflowHelper
    {
        private string _apiBaseUri = string.Empty;
        private string _bearerToken = string.Empty;
        public WorkflowHelper()
        {
            // TODO Get URI from appsetting.json 
            _apiBaseUri = "http://localhost:5001/api/";
            // TODO get BearerToken

        }

        public void WorkItemError(string EnrollmentGuid, string Info)
        { 
            // TODO 
            // Do something with the error to get human help!!!!

        }
        public void WorkItemFatalError(string EnrollmentGuid, string Info)
        {
            // TODO 
            // Do something with the error to get human help!!!!

        }

        public async Task<MessageTransactionResponse> DoWorkItem(string ApiAction)
        {
            HttpClient Client = ApiClient();
            HttpRequestMessage Request = ApiGetRequest(ApiAction);
            HttpResponseMessage Response = await Client.SendAsync(Request);
            var ResponseJson = await Response.Content.ReadAsStringAsync();
            MessageTransactionResponse RVal = JsonConvert.DeserializeObject<MessageTransactionResponse>(await Response.Content.ReadAsStringAsync());
            return RVal;
        }

        public async Task<string> UpdateEnrollmentStatus(string EnrollmentGuid, EnrollmentStatus status)
        {
            HttpClient Client = ApiClient();
            HttpRequestMessage Request = ApiPutRequest("Enrollment/UpdateEnrollmentStatus/" + EnrollmentGuid + "/" + (int) status );
            HttpResponseMessage Response = await Client.SendAsync(Request);
            var ResponseJson = await Response.Content.ReadAsStringAsync();
            return ResponseJson;
        }



        public async Task<string> CreateWozStudentLogin(VendorStudentLoginDto StudentLogin,string EnrollmentGuid)
        {

            string StudentLoginJson = Newtonsoft.Json.JsonConvert.SerializeObject(StudentLogin);
            HttpClient Client = ApiClient();
            HttpRequestMessage Request = ApiPostRequest("woz/SaveWozStudentLogin/" + EnrollmentGuid, StudentLoginJson);
            HttpResponseMessage Response = await Client.SendAsync(Request);
            var ResponseJson = await Response.Content.ReadAsStringAsync();
            return ResponseJson;
        }




        public void ParseWozEnrollmentResource( string WozTransactionResponse, ref string ExeterId, ref string RegistrationUrl )
        {
            JObject WozJson = JObject.Parse(WozTransactionResponse);
            string WozResourceStr = (string)WozJson["resource"];
            var WozResourceObject = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(WozResourceStr);
            ExeterId = WozResourceObject.exeterId;
            RegistrationUrl = WozResourceObject.registrationUrl;
        }



        private HttpRequestMessage ApiPutRequest(string ApiAction, string Content = "")
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, _apiBaseUri + ApiAction);

            if ( Content.Length > 0 )
            {
                request.Content = new StringContent(Content);
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            }
                
            return request;
        }

        private  HttpRequestMessage ApiPostRequest(string ApiAction, string Content)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, _apiBaseUri + ApiAction)
            {
                Content = new StringContent(Content)
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return request;
        }

        private HttpRequestMessage ApiGetRequest(string ApiAction)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, _apiBaseUri + ApiAction);
            return request;

        }


        private HttpClient ApiClient()
        {
            HttpClient client = new HttpClient();
            //  client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            return client;

        }


    }
}
