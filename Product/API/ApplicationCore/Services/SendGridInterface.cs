using AutoMapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.Models;
using UpDiddyLib.Dto.Marketing;
using UpDiddyLib.Helpers;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class SendGridInterface : BusinessVendorBase, ISendGridInterface
    {
        #region Constructor
        public SendGridInterface(
            UpDiddyDbContext context, 
            IMapper mapper, 
            Microsoft.Extensions.Configuration.IConfiguration configuration, 
            ILogger sysLog, 
            IHttpClientFactory httpClientFactory,
            string SendGridSubaccountAppsettingKey)
        {
            _db = context;
            _mapper = mapper;
            _apiBaseUri = configuration["SysEmail:ApiUrl"];
            _accessToken = configuration[SendGridSubaccountAppsettingKey];
            _syslog = sysLog;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;

        }
        #endregion


        #region Contacts

        public HttpResponseMessage AddContacts(IList<EmailContactDto> Contacts, ref string ResponseJson)
        {
            string Content = Newtonsoft.Json.JsonConvert.SerializeObject(Contacts);
            HttpClient client = CreateSendGridPostClient();
            HttpRequestMessage Request = CreateSendGridPostRequest("/contactdb/recipients", Content);
            HttpResponseMessage Response = AsyncHelper.RunSync<HttpResponseMessage>(() => client.SendAsync(Request));
            ResponseJson = AsyncHelper.RunSync<string>(() => Response.Content.ReadAsStringAsync());
            if (Response.StatusCode == HttpStatusCode.Created)
            {
                dynamic listInfo = JsonConvert.DeserializeObject(ResponseJson);
                JObject listObject = JObject.Parse(ResponseJson);
                ResponseJson = listObject["persisted_recipients"].ToString();
            }
            return Response;
        }
        #endregion

        #region Contact Lists 

        public HttpResponseMessage CreateContactList(SendGridListDto ListInfo, ref string ResponseJson)
        {
            string Content = Newtonsoft.Json.JsonConvert.SerializeObject(ListInfo);
            HttpClient client = CreateSendGridPostClient();
            HttpRequestMessage Request = CreateSendGridPostRequest("contactdb/lists", Content);
            HttpResponseMessage Response = AsyncHelper.RunSync<HttpResponseMessage>(() => client.SendAsync(Request));

            ResponseJson = AsyncHelper.RunSync<string>(() => Response.Content.ReadAsStringAsync());
            return Response;
        }


        public HttpResponseMessage GetContactLists(ref string ResponseJson)
        {    
            HttpClient client = CreateSendGridGetClient();
            HttpRequestMessage Request = CreateSendGridGetRequest("contactdb/lists");
            HttpResponseMessage Response = AsyncHelper.RunSync<HttpResponseMessage>(() => client.SendAsync(Request));
            ResponseJson = AsyncHelper.RunSync<string>(() => Response.Content.ReadAsStringAsync());
            if (Response.StatusCode == HttpStatusCode.OK)
            {
                dynamic listInfo = JsonConvert.DeserializeObject(ResponseJson);
                JObject listObject = JObject.Parse(ResponseJson);
                ResponseJson = listObject["lists"].ToString();             
            }
            return Response;
        }

        public HttpResponseMessage GetContactList(string ListName, ref string ResponseJson)
        {
            HttpClient client = CreateSendGridGetClient();
            HttpRequestMessage Request = CreateSendGridGetRequest("contactdb/lists");
            HttpResponseMessage Response = AsyncHelper.RunSync<HttpResponseMessage>(() => client.SendAsync(Request));
            ResponseJson = AsyncHelper.RunSync<string>(() => Response.Content.ReadAsStringAsync());
            if ( Response.StatusCode == HttpStatusCode.OK)
            {
                dynamic listInfo = JsonConvert.DeserializeObject(ResponseJson);
                JObject listObject = JObject.Parse(ResponseJson);
                string theLists = listObject["lists"].ToString();                
                IList<SendGridListDto> theList = JsonConvert.DeserializeObject<IList<SendGridListDto>>(theLists);
                foreach ( SendGridListDto listDto in theList)
                {
                    if ( listDto.name == ListName)
                    {
                        ResponseJson = Newtonsoft.Json.JsonConvert.SerializeObject(listDto);
                        return Response;
                    }
                }
                ResponseJson = ListName + " was not found";
                Response.StatusCode = HttpStatusCode.NotFound;
            }
            return Response;
        }

        public HttpResponseMessage AddContactsToList(string ListId, IList<EmailContactDto> Contacts, ref string ResponseJson)
        {
            string Content = string.Empty;
            AddContacts(Contacts, ref Content);

            HttpClient client = CreateSendGridPostClient();
            HttpRequestMessage Request = CreateSendGridPostRequest("/contactdb/lists/" +  ListId + "/recipients", Content);
            HttpResponseMessage Response = AsyncHelper.RunSync<HttpResponseMessage>(() => client.SendAsync(Request));
            ResponseJson = AsyncHelper.RunSync<string>(() => Response.Content.ReadAsStringAsync());        
            return Response;
        }


        public HttpResponseMessage CreateListAndAddContacts(string ListName, IList<EmailContactDto> Contacts, ref string ResponseJson)
        {
            string Content = string.Empty;
            // add the contacts
            AddContacts(Contacts, ref Content);
            // create the list and get its id 
            string ListCreateResult = string.Empty;
            SendGridListDto ListDto = new SendGridListDto
            {
                name = ListName,
                id = 0,
                recipient_count = 0               
            };
            
            // try to create the list as a new list 
            if ( CreateContactList(ListDto, ref ListCreateResult).StatusCode != HttpStatusCode.Created )
            {
                // try to locate an existing list 
                if (GetContactList(ListName, ref ListCreateResult).StatusCode != HttpStatusCode.OK)
                {
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        ReasonPhrase = "Unable to create send grid list"
                    };
                }
            }
            
            dynamic listInfo = JsonConvert.DeserializeObject(ListCreateResult);
            JObject listObject = JObject.Parse(ListCreateResult);
            string ListId = listObject["id"].ToString();

            HttpClient client = CreateSendGridPostClient();
            HttpRequestMessage Request = CreateSendGridPostRequest("/contactdb/lists/" + ListId + "/recipients", Content);
            HttpResponseMessage Response = AsyncHelper.RunSync<HttpResponseMessage>(() => client.SendAsync(Request));
            ResponseJson = AsyncHelper.RunSync<string>(() => Response.Content.ReadAsStringAsync());
            return Response;
        }


        #endregion

        #region Utility Functions

        private HttpResponseMessage ExecuteSendGridPost(string ApiAction, string Content, ref string ResponseJson)
        {
            HttpClient client = CreateSendGridPostClient();
            HttpRequestMessage SendGridRequest = CreateSendGridPostRequest(ApiAction, Content);
            HttpResponseMessage SendGridResponse = AsyncHelper.RunSync<HttpResponseMessage>(() => client.SendAsync(SendGridRequest));
            ResponseJson = AsyncHelper.RunSync<string>(() => SendGridResponse.Content.ReadAsStringAsync());
            return SendGridResponse;

        }

        private HttpResponseMessage ExecuteSendGridGet(string ApiAction, ref string ResponseJson)
        {

            HttpClient client = CreateSendGridGetClient();
            HttpRequestMessage SendGridRequest = CreateSendGridGetRequest(ApiAction);
            HttpResponseMessage SendGridResponse = AsyncHelper.RunSync<HttpResponseMessage>(() => client.SendAsync(SendGridRequest));
            ResponseJson = AsyncHelper.RunSync<string>(() => SendGridResponse.Content.ReadAsStringAsync());
            return SendGridResponse;
        }


        private HttpRequestMessage CreateSendGridPostRequest(string ApiAction, string Content)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, _apiBaseUri + ApiAction)
            {
                Content = new StringContent(Content)
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return request;
        }

        private HttpRequestMessage CreateSendGridGetRequest(string ApiAction)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, _apiBaseUri + ApiAction);
            return request;

        }

        private HttpClient CreateSendGridPostClient()
        {
            HttpClient client = _httpClientFactory.CreateClient(Constants.HttpPostClientName);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            return client;

        }

        private HttpClient CreateSendGridGetClient()
        {
            HttpClient client = _httpClientFactory.CreateClient(Constants.HttpGetClientName);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            return client;

        }
 
 
    

    }

    #endregion




}
 
