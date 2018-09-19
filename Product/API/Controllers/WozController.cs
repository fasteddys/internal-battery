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

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UpDiddyApi.Controllers
{
    // TODO Use Authorize 
    // [Authorize]
    public class WozController : Controller
    {


        private readonly UpDiddyDbContext _db = null;
        private readonly IMapper _mapper;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private readonly string _queueConnection = string.Empty;
        private readonly CCQueue _queue = null;
        private  readonly string _apiBaseUri = String.Empty;
        private readonly string _accessToken = String.Empty;

        public WozController(UpDiddyDbContext db, IMapper mapper, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            _db = db;
            _mapper = mapper;
            _configuration = configuration;
            _queueConnection = _configuration["CareerCircleQueueConnection"];
            _queue = new CCQueue("ccmessagequeue", _queueConnection);
            // TODO Move to Vault 
            _apiBaseUri = "https://clientapi.qa.exeterlms.com/v1/";
            // _accessToken = "d4672f0de07b8f25b473def953de63d6f5a2971f8bdd0b87d0b3a5abe9116569";
            _accessToken = _configuration["WozAccessToken"];

        }


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


         
        [HttpGet]
        [Route("api/[controller]/EnrollStudent/{EnrollmentGuid}")]
        public async Task<IActionResult> EnrollStudent(string EnrollmentGuid)
        {
            // TODO drive this off enrollment record 
            WozStudent Student = new WozStudent()
            {
                 firstName = "John",
                 lastName = "Smith",
                 emailAddress = "john.smith1@bigcompany.org",
                 acceptedTermsOfServiceDocumentId = 1,
                 suppressRegistrationEmail = true                 
            };

            string Json = Newtonsoft.Json.JsonConvert.SerializeObject(Student);

            HttpClient client = WozClient();
            HttpRequestMessage request = WozPostRequest("users", Json);           
            HttpResponseMessage response = await client.SendAsync(request);
            var ResponseJson = await response.Content.ReadAsStringAsync();

            // TODO fix error of non-json being returned 
            dynamic ResponseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(ResponseJson);           
            return Ok(ResponseJson);

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

        private HttpClient WozClient()
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            return client;

        }





    }
}
