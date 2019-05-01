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
using static Google.Apis.CloudTalentSolution.v3.ProjectsResource.ClientEventsResource;
using Google.Apis.CloudTalentSolution.v3.Data;
using UpDiddyApi.ApplicationCore.Services.GoogleJobs;
using Enum = System.Enum;
using MiniGuids;

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
        /// Remove job from cloud talent solution
        /// </summary>
        /// <param name="jobPosting"></param>
        /// <returns></returns>
        public bool RemoveJobFromIndex(JobPosting jobPosting)
        {
            try
            {

                bool isIndexed = false;
                if ( jobPosting.CloudTalentUri != null && string.IsNullOrEmpty(jobPosting.CloudTalentUri.Trim()) == false )
                {
                    isIndexed = true;
                    _jobServiceClient.Projects.Jobs.Delete(jobPosting.CloudTalentUri).Execute();
                }
                    
                // Update job posting with index error
                jobPosting.IsDeleted = 1;
                if (isIndexed)
                    jobPosting.CloudTalentIndexInfo = "Deleted on " + Utils.ISO8601DateString(DateTime.Now);
                else 
                    jobPosting.CloudTalentIndexInfo = "Deleted on " + Utils.ISO8601DateString(DateTime.Now) + " (not google indexed)";

                jobPosting.CloudTalentIndexStatus = (int)JobPostingIndexStatus.DeletedFromIndex;
                jobPosting.ModifyDate = DateTime.UtcNow;
                _db.SaveChanges();

                return true;
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
        /// Update the job in the google cloud talent solution
        /// </summary>
        /// <param name="jobPosting"></param>
        /// <returns></returns>

        public CloudTalentSolution.Job ReIndexJob(JobPosting jobPosting)
        {
            try
            {
                CloudTalentSolution.Job TalentCloudJob = JobMappingHelper.CreateGoogleJob(_db, jobPosting);
                CloudTalentSolution.UpdateJobRequest UpdateJobRequest = new CloudTalentSolution.UpdateJobRequest();
                UpdateJobRequest.Job = TalentCloudJob;                        
                CloudTalentSolution.Job jobCreated = _jobServiceClient.Projects.Jobs.Patch(UpdateJobRequest, TalentCloudJob.Name).Execute();
                // Update job posting with index error
                jobPosting.CloudTalentUri = jobCreated.Name;
                jobPosting.CloudTalentIndexInfo = "ReIndexed on " + Utils.ISO8601DateString(DateTime.Now);
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
                CloudTalentSolution.Job TalentCloudJob = JobMappingHelper.CreateGoogleJob(_db, jobPosting);
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


        public bool AddJobToCloudTalent(UpDiddyDbContext db, Guid jobPostingGuid)
        {
            try
            {
                JobPosting jobPosting = JobPostingFactory.GetJobPostingByGuidWithRelatedObjects(db, jobPostingGuid);
                // validate we have good data 
                if (jobPosting == null || jobPosting.Company == null)
                    return false;
                // validate the company is known to google, if not add it to the cloud talent 
                if (string.IsNullOrEmpty(jobPosting.Company.CloudTalentUri))
                    IndexCompany(jobPosting.Company);

                // index the job to google 
                CloudTalentSolution.Job job = IndexJob(jobPosting);
                return true;
            }
            catch
            {
                return false;
            }
        }


        public bool UpdateJobToCloudTalent(UpDiddyDbContext db, Guid jobPostingGuid)
        {
            try
            {
                JobPosting jobPosting = JobPostingFactory.GetJobPostingByGuidWithRelatedObjects(db, jobPostingGuid);
                    // validate we have good data 
                if (jobPosting == null || jobPosting.Company == null)
                    return false;
                // validate the company is known to google, if not add it to the cloud talent 
                if (string.IsNullOrEmpty(jobPosting.Company.CloudTalentUri))
                    IndexCompany(jobPosting.Company);

                // index the job to google 
                CloudTalentSolution.Job job = ReIndexJob(jobPosting);
                return true;
            }
            catch
            {
                return false;
            }
        }



        public bool DeleteJobFromCloudTalent(UpDiddyDbContext db, Guid jobPostingGuid)
        {
            try
            {
                JobPosting jobPosting = JobPostingFactory.GetJobPostingByGuid(db, jobPostingGuid);
                // validate we have good data 
                if (jobPosting == null)
                    return false;

                // index the job to google 
                return RemoveJobFromIndex(jobPosting);
            }
            catch
            {
                return false;
            }
        }


        #endregion

        #region job searching 

        public CloudTalentSolution.SearchJobsRequest CreateJobSearchRequest(JobQueryDto jobQuery)
        {
            // todo add better meta data 
            CloudTalentSolution.RequestMetadata requestMetadata = new CloudTalentSolution.RequestMetadata()
            {
                UserId = "CareerCircle.com",
                SessionId = "n/a",
                Domain = "www.careercircle.com"

            };

            CloudTalentSolution.JobQuery cloudTalentJobQuery = new CloudTalentSolution.JobQuery();
            // add keywords 
            if ( string.IsNullOrEmpty(jobQuery.Keywords) == false )
            {                
                cloudTalentJobQuery.Query = jobQuery.Keywords;
            }

     

            if ( jobQuery.Lat != 0  && jobQuery.Lng != 0)
            {
                cloudTalentJobQuery.CommuteFilter = new CloudTalentSolution.CommuteFilter()
                {
                    AllowImpreciseAddresses = !jobQuery.PreciseAddress,
                    CommuteMethod = jobQuery.PublicTransit ? "TRANSIT" : "DRIVING",
                    StartCoordinates = new CloudTalentSolution.LatLng()
                    {
                        Latitude = jobQuery.Lat,
                        Longitude = jobQuery.Lng,
                    },
                    // Google's Duraton.ToString is including  escaped double quotes.  For now just 
                    // format in the required format 
                    TravelDuration = (jobQuery.CommuteTime * 60).ToString() + "s",
                    RoadTraffic = jobQuery.RushHour ? "BUSY_HOUR" : "TRAFFIC_FREE"
                
                };
            }
            else // not commute search 
            {
                // us a country code to help google dis-ambiguate state abbreviatons, etc.   
                string regionCode = string.IsNullOrEmpty(jobQuery.Country) ? "us" : jobQuery.Country;
                // add locations filters.  give preference to the free format location field if it's 
                // defined, if not use city state parameters if they have been defined 
                string addressInfo = string.Empty;
                if (string.IsNullOrEmpty(jobQuery.Location) == false)
                    addressInfo = jobQuery.Location;
                else
                {
                    // build address with comma placeholders to help google parse the location
                    addressInfo = jobQuery.StreetAddress + ", " + jobQuery.City + ", " + jobQuery.Province + ", " + regionCode;
                    addressInfo = addressInfo.Trim();
                }
                // add location filter if any address information has been provided  
                if (string.IsNullOrEmpty(addressInfo) == false ||  string.IsNullOrEmpty(jobQuery.Province) == false )
                {
                    CloudTalentSolution.LocationFilter locationFilter = new CloudTalentSolution.LocationFilter()
                    {                
                        Address = addressInfo,
                        DistanceInMiles = jobQuery.SearchRadius,
                        RegionCode = regionCode                                              
                    };
 
                    cloudTalentJobQuery.LocationFilters = new List<CloudTalentSolution.LocationFilter>()
                    {
                        locationFilter                        
                    };
                    
                }
            }
          
            // publish time range 
            if ( string.IsNullOrEmpty(jobQuery.DatePublished) == false)
            {
                cloudTalentJobQuery.PublishTimeRange = GetPublishTimeRange(jobQuery.DatePublished);
            }

            // company name 
            if (string.IsNullOrEmpty(jobQuery.CompanyName) == false)
            {
                string[] companyNames = { jobQuery.CompanyName };
                cloudTalentJobQuery.CompanyDisplayNames = companyNames;                
            }

            // custom attribute filters 
            // todo add more custom filter capabilities as necessary 

            // add skills 
            string attributeFilters = string.Empty;
            if ( string.IsNullOrEmpty (jobQuery.Skill) == false )
                attributeFilters = "LOWER(Skills) = \"" + jobQuery.Skill.Trim().ToLower()  + "\"";

            // add industry 
            if (string.IsNullOrEmpty(jobQuery.Industry) == false)
            {
                if (attributeFilters.Length > 0)
                    attributeFilters += " AND ";

                attributeFilters += "LOWER(Industry) = \"" + jobQuery.Industry.Trim().ToLower() + "\"";
            }

            // add job category 
            if (string.IsNullOrEmpty(jobQuery.JobCategory) == false)
            {
                if (attributeFilters.Length > 0)
                    attributeFilters += " AND ";

                attributeFilters += "LOWER(JobCategory) = \"" + jobQuery.JobCategory.Trim().ToLower() + "\"";
            }

            // add education level 
            if (string.IsNullOrEmpty(jobQuery.EducationLevel) == false)
            {
                if (attributeFilters.Length > 0)
                    attributeFilters += " AND ";

                attributeFilters += "LOWER(EducationLevel) = \"" + jobQuery.EducationLevel.Trim().ToLower() + "\"";
            }

            // add employment type 
            if (string.IsNullOrEmpty(jobQuery.EmploymentType) == false)
            {
                if (attributeFilters.Length > 0)
                    attributeFilters += " AND ";

                attributeFilters += "LOWER(EmploymentType) = \"" + jobQuery.EmploymentType.Trim().ToLower() + "\"";

            }

            // add employment type 
            if (string.IsNullOrEmpty(jobQuery.ExperienceLevel) == false)
            {
                if (attributeFilters.Length > 0)
                    attributeFilters += " AND ";

                attributeFilters += "LOWER(ExperienceLevel) = \"" + jobQuery.ExperienceLevel.Trim().ToLower() + "\"";
            }

            // Add Custom Attribute Filter 
            if ( attributeFilters.Length > 0 )
                cloudTalentJobQuery.CustomAttributeFilter = attributeFilters;
 
            // Add histograms 
            CloudTalentSolution.HistogramFacets histogramFacets = new CloudTalentSolution.HistogramFacets()
            {
                SimpleHistogramFacets = new List<String>
                {

                    "DATE_PUBLISHED",
                    "ADMIN_1", // Region such as State or Province
                    "CITY",
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
                      Key = "ExperienceLevel",
                      StringValueHistogram = true
                   },
                   new CloudTalentSolution.CustomAttributeHistogramRequest()
                   {
                      Key = "EducationLevel",
                      StringValueHistogram = true
                   },
                   new CloudTalentSolution.CustomAttributeHistogramRequest()
                   {
                      Key = "EmploymentType",
                      StringValueHistogram = true
                   },
                   new CloudTalentSolution.CustomAttributeHistogramRequest()
                   {
                      Key = "Industry",
                      StringValueHistogram = true
                   },
                   new CloudTalentSolution.CustomAttributeHistogramRequest()
                   {
                      Key = "JobCategory",
                      StringValueHistogram = true
                   }

               }
            };

            // Build search request 
            CloudTalentSolution.SearchJobsRequest searchJobRequest = new CloudTalentSolution.SearchJobsRequest()
            {
                RequestMetadata = requestMetadata,
                JobQuery = cloudTalentJobQuery,
                SearchMode = "JOB_SEARCH",
                HistogramFacets = histogramFacets, 
                PageSize = jobQuery.PageSize,
                Offset = jobQuery.PageSize * (jobQuery.PageNum - 1),
                OrderBy = jobQuery.OrderBy
               

            };

            

            return searchJobRequest;
        }



        /// <summary>
        /// Search the cloud talent solution for jobs 
        /// </summary>
        /// <param name="jobQuery"></param>
        /// <returns></returns>
        public JobSearchResultDto Search(JobQueryDto jobQuery)
        {            
            // map jobquery to cloud talent search request 
            DateTime startSearch = DateTime.Now;
            CloudTalentSolution.SearchJobsRequest searchJobRequest = CreateJobSearchRequest(jobQuery);
            
            // search the cloud talent 
            CloudTalentSolution.SearchJobsResponse searchJobsResponse = _jobServiceClient.Projects.Jobs.Search(searchJobRequest, _projectPath).Execute();

            // map cloud talent results to cc search results 
            DateTime startMap= DateTime.Now;
            JobSearchResultDto rval = JobMappingHelper.MapSearchResults(_syslog, _mapper,_configuration, searchJobsResponse, jobQuery);
            DateTime stopMap = DateTime.Now;

            // calculate search timing metrics 
            TimeSpan intervalTotalSearch = stopMap - startSearch;
            TimeSpan intervalSearchTime = startMap - startSearch;
            TimeSpan intervalMapTime = stopMap - startMap;

            // assign search metrics to search results 
            rval.SearchTimeInMilliseconds = intervalTotalSearch.TotalMilliseconds;
            rval.SearchQueryTimeInTicks = intervalSearchTime.Ticks;
            rval.SearchMappingTimeInTicks = intervalMapTime.Ticks;
            return rval;
        }


        #endregion

        #region ClientEvents
        /// <summary>
        /// Creates a client event in google to help improve job search via machine learning.
        /// </summary>
        /// <param name="requestId">string identifier returned by google for the request used</param>
        /// <param name="type"></param>
        /// <param name="jobNames"></param>
        /// <param name="parentEventId"></param>
        /// <returns>ClientEvent</returns>
        public async Task<ClientEvent> CreateClientEventAsync(string requestId, ClientEventType type, List<string> jobNames, string parentEventId = null)
        {
            string time = DateTime.UtcNow.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");
            JobEvent je = new JobEvent()
            {
                Jobs = jobNames,
                Type = Enum.GetName(typeof(ClientEventType), type).ToUpper(),
            };
            ClientEvent ce = new ClientEvent()
            {
                EventId = string.Format("{0}-{1}", DateTimeOffset.UtcNow.ToUnixTimeSeconds(), MiniGuid.NewGuid().ToString()),
                JobEvent = je,
                RequestId = requestId,
                CreateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                ParentEventId = parentEventId,

            };

            CreateClientEventRequest ccer = new CreateClientEventRequest()
            {
                ClientEvent = ce
            };
            CreateRequest request = _jobServiceClient.Projects.ClientEvents.Create(ccer, _projectPath);
            ce = await request.ExecuteAsync();
            return ce;
        }
        #endregion

        #region Helper functions

        static private CloudTalentSolution.TimestampRange GetPublishTimeRange( string timeRange )
        {
            CloudTalentSolution.TimestampRange rVal = new CloudTalentSolution.TimestampRange();
            rVal.EndTime = Utils.GetTimestampAsString(DateTime.Now);

            switch ( timeRange.ToLower() )
            {
                case "past_24_hours":
                    rVal.StartTime = Utils.GetTimestampAsString(DateTime.Now.AddHours(-24));
                    break;
                case "past_3_days":
                    rVal.StartTime = Utils.GetTimestampAsString(DateTime.Now.AddHours(-72));
                    break;
                case "past_week":
                    rVal.StartTime = Utils.GetTimestampAsString(DateTime.Now.AddDays(-7));
                    break;
                case "past_month":
                    rVal.StartTime = Utils.GetTimestampAsString(DateTime.Now.AddDays(-30));
                    break;
                default:
                    rVal.StartTime = Utils.GetTimestampAsString(DateTime.Now.AddDays(-365));
                    break;                    
            }
            return rVal;
        }

        #endregion
        
    }
}
