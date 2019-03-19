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
using Google.Apis.CloudTalentSolution.v3.Data;
using Google.Protobuf.WellKnownTypes;
using Google.Apis.CloudTalentSolution.v3;
using Google.Apis.Services;
using Google.Apis.Auth.OAuth2;
using UpDiddyApi.ApplicationCore.Factory;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class CloudTalent : BusinessVendorBase
    {

        private string _projectId = string.Empty;
        private string _projectPath = string.Empty;
        private CloudTalentSolutionService _jobServiceClient = null;
        private GoogleCredential _credential = null;

        #region Constructor
        public CloudTalent(UpDiddyDbContext context, IMapper mapper, Microsoft.Extensions.Configuration.IConfiguration configuration, ILogger sysLog, IHttpClientFactory httpClientFactory)
        {
            _db = context;
            _mapper = mapper;
            _apiBaseUri = configuration["SysEmail:ApiUrl"];
            _accessToken = configuration["SysEmail:ApiKey"];
            _syslog = sysLog;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;

            // cloud talent configuration
            _projectId = configuration["CloudTalent:Project"];
            _projectPath = configuration["CloudTalent:ProjectPath"];
            // must have path to service account json file created on the cloud.google.com defined 
            // in GOOGLE_APPLICATION_CREDENTIALS environmental variable
            _credential = GoogleCredential.GetApplicationDefaultAsync().Result;

            _jobServiceClient = new CloudTalentSolutionService(new BaseClientService.Initializer
            {
                HttpClientInitializer = _credential,
                GZipEnabled = false
            });
        }
        #endregion

        public Job IndexJob(JobPosting jobPosting)
        {            
            try
            {
                Job TalentCloudJob = JobPostingFactory.ToGoogleJob(jobPosting);           
                CreateJobRequest CreateJobRequest = new CreateJobRequest();
                CreateJobRequest.Job = TalentCloudJob;
                // 
                Job jobCreated = _jobServiceClient.Projects.Jobs.Create(CreateJobRequest, _projectPath).Execute();
 
                return jobCreated;
            }
            catch (Exception e)
            {
                _syslog.LogError(e, "CloudTalent.IndexJob Error", jobPosting);
                throw e;
            }
        }




    }
}
