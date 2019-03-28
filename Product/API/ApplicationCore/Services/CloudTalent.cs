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
using CloudTalentSolution = Google.Apis.CloudTalentSolution.v3.Data;
using Google.Protobuf.WellKnownTypes;
using Google.Apis.CloudTalentSolution.v3;
using Google.Apis.Services;
using Google.Apis.Auth.OAuth2;
using UpDiddyApi.ApplicationCore.Factory;
using UpDiddyLib.Dto;
using UpDiddyApi.Helpers.Job;

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

            // Specify the Service scope.
            if (_credential.IsCreateScopedRequired)
            {
                _credential = _credential.CreateScoped(new[]
                {
                    Google.Apis.CloudTalentSolution.v3.CloudTalentSolutionService.Scope.Jobs
                });
            }

            _jobServiceClient = new CloudTalentSolutionService(new BaseClientService.Initializer
            {
                HttpClientInitializer = _credential,
                GZipEnabled = false
            });

        }
        #endregion



        #region Job Indexing
        /// <summary>
        /// Delete a job from the google index via its google URI pth
        /// </summary>
        /// <param name="googleUri"></param>
        /// <returns></returns>
        public bool DeleteJobFromIndex(string googleUri)
        {
            try
            {
                _jobServiceClient.Projects.Jobs.Delete(googleUri).Execute();
 
            }
            catch (Exception e)
            {   
                throw e;
            }


            return true;
        }

        /// <summary>
        /// Update the job in the google clouse 
        /// </summary>
        /// <param name="jobPosting"></param>
        /// <returns></returns>

        public CloudTalentSolution.Job ReIndexJob(JobPosting jobPosting)
        {
            try
            {
                CloudTalentSolution.Job TalentCloudJob = JobHelper.CreateGoogleJob(_db, jobPosting);
                CloudTalentSolution.UpdateJobRequest UpdateJobRequest = new CloudTalentSolution.UpdateJobRequest();
                UpdateJobRequest.Job = TalentCloudJob;                        
                CloudTalentSolution.Job jobCreated = _jobServiceClient.Projects.Jobs.Patch(UpdateJobRequest, TalentCloudJob.Name).Execute();
                // Update job posting with index error
                jobPosting.CloudTalentUri = jobCreated.Name;
                jobPosting.CloudTalentIndexInfo = "Indexed on " + Utils.ISO8601DateString(DateTime.Now);
                jobPosting.CloudTalentIndexStatus = (int)JobPostingIndexStatus.Indexed;
                _db.SaveChanges();

                return jobCreated;
            }
            catch (Exception e)
            {
                // Update job posting with index error
                jobPosting.CloudTalentIndexInfo = e.Message;
                jobPosting.CloudTalentIndexStatus = (int)JobPostingIndexStatus.IndexError;
                _db.SaveChanges();
                _syslog.LogError(e, "CloudTalent.IndexJob Error", e, jobPosting);
                throw e;
            }
        }


        /// <summary>
        /// Add a job posting to the google cloud talent solution
        /// </summary>
        /// <param name="jobPosting"></param>
        /// <returns></returns>
        public CloudTalentSolution.Job IndexJob(JobPosting jobPosting)
        {            
            try
            {
                CloudTalentSolution.Job TalentCloudJob = JobHelper.CreateGoogleJob(_db, jobPosting);
                CloudTalentSolution.CreateJobRequest CreateJobRequest = new CloudTalentSolution.CreateJobRequest();
                CreateJobRequest.Job = TalentCloudJob;
                // "Google.Apis.Requests.RequestError\r\nInvalid value at 'job.posting_expire_time' (type.googleapis.com/google.protobuf.Timestamp), Field 'postingExpireTime', Illegal timestamp format; timestamps must end with 'Z' or have a valid timezone offset. [400]\r\nErrors [\r\n\tMessage[Invalid value at 'job.posting_expire_time' (type.googleapis.com/google.protobuf.Timestamp), Field 'postingExpireTime', Illegal timestamp format; timestamps must end with 'Z' or have a valid timezone offset.] Location[ - ] Reason[badRequest] Domain[global]\r\n]\r\n"
                CloudTalentSolution.Job jobCreated = _jobServiceClient.Projects.Jobs.Create(CreateJobRequest, _projectPath).Execute();
                // Update job posting with index error
                jobPosting.CloudTalentUri = jobCreated.Name;
                jobPosting.CloudTalentIndexInfo = "Indexed on " + Utils.ISO8601DateString(DateTime.Now);
                jobPosting.CloudTalentIndexStatus = (int)JobPostingIndexStatus.Indexed;
                _db.SaveChanges();

                return jobCreated;
            }
            catch (Exception e)
            {
                // Update job posting with index error
                jobPosting.CloudTalentIndexInfo = e.Message;
                jobPosting.CloudTalentIndexStatus = (int) JobPostingIndexStatus.IndexError;
                _db.SaveChanges();
                _syslog.LogError(e, "CloudTalent.IndexJob Error", e, jobPosting);
                throw e;
            }
        }

        /// <summary>
        /// Add company to google cloud talent solution
        /// </summary>
        /// <param name="company"></param>
        /// <returns></returns>
        public CloudTalentSolution.Company IndexCompany(Models.Company company)
        {
            try
            {
                bool isHiringAgency = false;
                if (company.IsHiringAgency == 1)
                    isHiringAgency = true;

                CloudTalentSolution.Company companyToBeCreated = new CloudTalentSolution.Company()
                {
                    DisplayName = company.CompanyName,
                    ExternalId = company.CompanyGuid.ToString(),
                    HiringAgency = isHiringAgency
                };

                CloudTalentSolution.CreateCompanyRequest createCompanyRequest = new CloudTalentSolution.CreateCompanyRequest();
                createCompanyRequest.Company = companyToBeCreated;
                CloudTalentSolution.Company companyCreated = _jobServiceClient.Projects.Companies.Create(createCompanyRequest, _projectPath).Execute();

                company.CloudTalentUri = companyCreated.Name;
                company.CloudTalentIndexInfo = "Indexed on " + Utils.ISO8601DateString(DateTime.Now);
                company.CloudTalentIndexStatus = (int)JobPostingIndexStatus.Indexed;
                _db.SaveChanges();
                return companyCreated;
            }
            catch (Exception e)
            {
                // Update job posting with index error
                company.CloudTalentIndexInfo = e.Message;
                company.CloudTalentIndexStatus = (int)JobPostingIndexStatus.IndexError;
                _db.SaveChanges();
                _syslog.LogError(e, "CloudTalent.IndexJob Error", e, company);
                throw e;
            }
        }

        #endregion


        #region job searching 


        public JobSearchResultDto Search()
        {
            CloudTalentSolution.RequestMetadata requestMetadata = new CloudTalentSolution.RequestMetadata()
            {
                UserId = "CareerCircle.com",
                SessionId = "n/a",
                Domain = "www.careercircle.com"
            };

            // Create job query 
            CloudTalentSolution.JobQuery jobQuery = new CloudTalentSolution.JobQuery();
            // TODO JAB  pass in query 
            string query = "javascript c#";
          //  jobQuery.Query = query;

            // Add CompanyFilters - Not sure if this is company URI name or company display name 
            string companyName = "";
            //jobQuery.CompanyNames = companyName;

            // Add Custom Attribute Filter 
            string customAttributeFilter = "LOWER(Skills) = \"javascript\"";
           // jobQuery.CustomAttributeFilter = customAttributeFilter;

            // Add Location Filter             
            CloudTalentSolution.LocationFilter locationFilter = new CloudTalentSolution.LocationFilter()
            {
                // Address = ,
                Address = "Towson MD",
                DistanceInMiles = 300

            };

            jobQuery.LocationFilters = new List<CloudTalentSolution.LocationFilter>()
            {
                locationFilter
            };

            // Add histograms 
            CloudTalentSolution.HistogramFacets histogramFacets = new CloudTalentSolution.HistogramFacets()
            {
                SimpleHistogramFacets = new List<String>
                {
                    "COMPANY_ID",
                    "COUNTRY",
                    "EMPLOYMENT_TYPE",
                    "COMPANY_SIZE",
                    "DATE_PUBLISHED",
                    "EDUCATION_LEVEL",
                    "EXPERIENCE_LEVEL",
                    "ADMIN_1", // Region such as State or Province
                    "CITY",
                    "EMPLOYMENT_TYPE",
                    "CATEGORY",
                    "LOCALE",
                    "LANGUAGE",
                    "CITY_COORDINATE",
                    "ADMIN_1_COUNTRY",
                    "COMPANY_DISPLAY_NAME",
                    "BASE_COMPENSATION_UNIT"

                },
                CustomAttributeHistogramFacets = new List<CloudTalentSolution.CustomAttributeHistogramRequest>
               {
                   new CloudTalentSolution.CustomAttributeHistogramRequest()
                   {
                      Key = "Skills",
                      StringValueHistogram = true
                   },
                   new CloudTalentSolution.CustomAttributeHistogramRequest()
                   {
                      Key = "ApplicationDeadlineUTC",
                      StringValueHistogram = true
                   },
                    new CloudTalentSolution.CustomAttributeHistogramRequest()
                   {
                      Key = "ExperienceLevel",
                      StringValueHistogram = true
                   },
                   new CloudTalentSolution.CustomAttributeHistogramRequest()
                   {
                      Key = "EducationLevel",
                      StringValueHistogram = true
                   }
                    
               }
            };

            // Build search request 
            CloudTalentSolution.SearchJobsRequest searchJobRequest = new CloudTalentSolution.SearchJobsRequest()
            {
                RequestMetadata = requestMetadata,
                JobQuery = jobQuery,
                SearchMode = "JOB_SEARCH",
                HistogramFacets = histogramFacets
            };
            CloudTalentSolution.SearchJobsResponse searchJobsResponse = _jobServiceClient.Projects.Jobs.Search(searchJobRequest, _projectPath).Execute();

      
            JobSearchResultDto rval =  JobHelper.MapSearchResults(_syslog, _mapper, searchJobsResponse);
            return rval;
        }





        #endregion





    }
}
