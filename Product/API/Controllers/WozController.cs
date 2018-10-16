using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyLib.MessageQueue;
using Newtonsoft;
using System.Reflection;
using System.Globalization;
using Newtonsoft.Json.Linq;
using UpDiddyLib.Helpers;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UpDiddyApi.Controllers
{
    // TODO Use Authorize 
    // [Authorize]
    public class WozController : Controller
    {

        #region Class Members
        private readonly UpDiddyDbContext _db = null;
        private readonly IMapper _mapper;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private readonly string _queueConnection = string.Empty;
        private readonly CCQueue _queue = null;
        private readonly string _apiBaseUri = String.Empty;
        private readonly string _accessToken = String.Empty;
        private WozTransactionLog _log = null;
        #endregion

        #region Constructor
        public WozController(UpDiddyDbContext db, IMapper mapper, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            _db = db;
            _mapper = mapper;
            _configuration = configuration;
            _queueConnection = _configuration["CareerCircleQueueConnection"];
            _queue = new CCQueue("ccmessagequeue", _queueConnection);      
            _apiBaseUri = _configuration["Woz:ApiUrl"];
            _accessToken = _configuration["WozAccessToken"];            
            _log = new WozTransactionLog();
        }
        #endregion

         

        #region Courses 
        // GET: api/<controller>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, _apiBaseUri + "courses");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            HttpResponseMessage response = await client.SendAsync(request);
            var ResponseJson = await response.Content.ReadAsStringAsync();
            return Ok(ResponseJson);

        }

        /*
        // TODO THIS ENDPOINT MUST BE REMOVED!!!!
        // GET: api/<controller>
        [HttpGet]
        [Route("api/[controller]/LoadWozCourses")]
        public async Task<IActionResult> LoadWozCourses()
        {


               string[] CourseCodes = {  "SWD100","SWD101","SWD102-CS","SWD102-JV","SWD103-AN","SWD103-RT",
                                         "SWD104-CS","SWD104-JS","SWD104-JV","SWD105","SWD106-RW","SWD107-AG",
                                         "SWD108-DE","SWD108-WS","DSO101","DSO102","DSO103-ME","DSO104","DSO105","DSO106-ML",
                                         "DSO106-MO","DSO107","DSO109","CSO100","CSO101","CSO102","CSO103","CSO104","CSO105","CSO108"
                                      };


            // string[] CourseCodes = { "SWD100" };



            string Name = string.Empty;
            string CourseCode = string.Empty;
            string Description = string.Empty;
            string Mp4 = string.Empty;
            string Mp4_720 = string.Empty;
            string Mp4_540 = string.Empty;
            string Mp4_360 = string.Empty;
            string ResourceURI = string.Empty;
            string ResourceKey = string.Empty;

            string CurrentCourseCode = string.Empty;
            string Json = string.Empty;

            foreach (string code in CourseCodes)
                {
                    try
                    {
                        CurrentCourseCode = code;
                        // TODO Blow away all current courses 
                       

                        Json = await GetCourseJson(code);
                        var WozO = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(Json);
                        ParseWozCourse(WozO, ref Description, ref Name, ref CourseCode, ref Mp4, ref Mp4_720, ref Mp4_540, ref Mp4_360, ref ResourceURI, ref ResourceKey);
                        Json = await GetCourseDescriptionJson(ResourceURI);

                        JObject WozJson = JObject.Parse(Json);
                        Newtonsoft.Json.Linq.JToken CourseDescription = WozJson[ResourceKey];
                        Description = CourseDescription.ToString();
                        Description = Description.Replace("<h1>Description</h1>", string.Empty);

                        Description = Utils.RemoveHTML(Description);
                        Description = Utils.RemoveNewlines(Description);

                        Course NewCourse = new Course()
                        {
                            CourseId = 0,
                            CourseGuid = Guid.NewGuid(),
                            Description = Description,
                            Code = CourseCode,
                            VideoUrl = Mp4_720,
                            Name = Name,
                            IsDeleted = 0,
                            ModifyDate = DateTime.Now,
                            CreateDate = DateTime.Now,
                            Slug = Name.Trim().Replace(" ", "-")
                        };
                        _db.Add(NewCourse);
                        Console.WriteLine(CurrentCourseCode + " is good");
                }
                    catch (Exception ex)
                    {
                        Console.WriteLine(CurrentCourseCode + " has failed.");
                        Console.WriteLine("Returned Json: " + Json);
                }

            }
         
           _db.SaveChanges();           
            return Ok();

        }

 

        private bool ParseWozCourse( dynamic WozO, ref string Description,
                                    ref string Name, ref string CourseCode,
                                    ref string Mp4, ref string Mp4_720, 
                                    ref string Mp4_540, ref string Mp4_360,
                                    ref string ResourceURI, ref string ResourceKey)
        {

            Description = WozO.description.ToString();
            Name = WozO.name.ToString();
            CourseCode = WozO.courseCode.ToString();

            foreach (var appendix in WozO.appendices)
            {
                var AType = (int)appendix.appendixType;
                
                if (AType == 1)
                {
                    ResourceURI = appendix.resourceUri;
                    ResourceKey = appendix.resourceKey;

                    foreach (var attr in appendix.attributes)
                    {
                        if (attr.key == "video-url-mp4")
                            Mp4 = attr.value.ToString();
                        else if (attr.key == "video-url-mp4-720")
                            Mp4_720 = attr.value.ToString();
                        else if (attr.key == "video-url-mp4-540")
                            Mp4_540 = attr.value.ToString();
                        else if (attr.key == "video-url-mp4-360")
                            Mp4_360 = attr.value.ToString();
                    }
                }
            }

            return true;
        }


        private async Task<string> GetCourseDescriptionJson(string ResourceUri)
        {
            HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, ResourceUri);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            HttpResponseMessage response = await client.SendAsync(request);
            var ResponseJson = await response.Content.ReadAsStringAsync();
            return ResponseJson;

        }



        private async Task<string> GetCourseJson(string CourseCode)
        {
            HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, _apiBaseUri + "courses/" + CourseCode);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            HttpResponseMessage response = await client.SendAsync(request);
            var ResponseJson = await response.Content.ReadAsStringAsync();
            return ResponseJson;

        }

    
    */

        #endregion

        #region Terms Of Service
        [HttpGet]
        [Route("api/[controller]/TermsOfService")]
        // TODO enable caching 
        // [ResponseCache(Duration = 600)]
        // Create or retreive a section for the the given course 
        public async Task<WozTermsOfServiceDto> TermsOfService()
        {
            
            WozTermsOfServiceDto RVal = new WozTermsOfServiceDto();

            try
            {                
                HttpClient client = WozClient();
                HttpRequestMessage WozRequest = WozGetRequest("tos");
                HttpResponseMessage WozResponse = await client.SendAsync(WozRequest);
                var ResponseJson = await WozResponse.Content.ReadAsStringAsync();

                if (WozResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    // dynamic ResponseObject  = System.Web.Helpers.Json.Decode(ResponseJson);
                    //dynamic ResponseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(ResponseJson);
                    var ResponseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(ResponseJson);

                    string TermsOfServiceId = ResponseObject.termsOfServiceDocumentId;
                    string Content = ResponseObject.termsOfServiceContent;
                    RVal.DocumentId = int.Parse(TermsOfServiceId);
                    RVal.TermsOfService = Utils.RemoveRedundantSpaces(Utils.RemoveNewlines(Utils.RemoveHTML(Content)));
                  

                    // See if the latest TOS from woz has been stored to our local DB
                    WozTermsOfService tos = _db.WozTermsOfService
                        .Where(t => t.IsDeleted == 0 && t.DocumentId == RVal.DocumentId)                
                        .FirstOrDefault();

                    // Add the latest version to our database if it's not there 
                    if ( tos == null )
                    {
                        WozTermsOfService NewTermsOfService = _mapper.Map<WozTermsOfService>(RVal);
                        _db.WozTermsOfService.Add(NewTermsOfService);
                        _db.SaveChanges();
                    }

                }
                else
                    RVal = LastGoodTermsOfService();
            
            }
            catch( Exception ex )
            {
                RVal = LastGoodTermsOfService();
            }
            return RVal;
        }

            #endregion

        #region Helper Functions


         private WozTermsOfServiceDto LastGoodTermsOfService()
         {
            WozTermsOfServiceDto RVal = null;
            WozTermsOfService tos = _db.WozTermsOfService
                .Where(t => t.IsDeleted == 0)
                .OrderByDescending(t => t.DocumentId)
                .FirstOrDefault();

            if (tos != null)
                RVal = _mapper.Map<WozTermsOfServiceDto>(tos);
                
           return RVal;            
         }


        private HttpRequestMessage WozPostRequest(string ApiAction, string Content)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, _apiBaseUri + ApiAction)
            {
               Content = new StringContent(Content)
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return request;
        }

        private HttpRequestMessage WozGetRequest(string ApiAction)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, _apiBaseUri + ApiAction);                   
            return request;

        }




        private HttpClient WozClient()
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            return client;

        }

        private MessageTransactionResponse CreateResponse( string ResponseJson, string Info, string Data, TransactionState State)
        {
            MessageTransactionResponse RVal =  new MessageTransactionResponse()
            {
                ResponseJson = ResponseJson,
                InformationalMessage = Info,
                Data = Data,
                State = State
            };

            string RValJson = Newtonsoft.Json.JsonConvert.SerializeObject(RVal);
            return RVal;
        }
        #endregion

    }
}
