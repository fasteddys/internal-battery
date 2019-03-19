using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyLib.Helpers;
using Google.Apis.CloudTalentSolution.v3.Data;
using Google.Protobuf.WellKnownTypes;
using Google.Apis.CloudTalentSolution.v3;
using Google.Apis.Services;
using Google.Apis.Auth.OAuth2;

namespace UpDiddyApi.ApplicationCore.Factory
{
    public class JobPostingFactory
    {
        static public Job ToGoogleJob(JobPosting jobPosting)
        {
            // Set default application instructions as required by Google
            ApplicationInfo applicationInfo = new ApplicationInfo()
            {
                Instruction = "Apply Now!",
            };

            // Create custom index attributes container  
            IDictionary<string, CustomAttribute> customAttributes = new Dictionary<string, CustomAttribute>();

            /* -------------------  todo add custom attributes as needed 
             * 


            // example of adding custom skills to job which will create a skills facet for search result that can be used to 
            // implement navigators

            CustomAttribute ca = new CustomAttribute()
            {
                Filterable = true,
                StringValues = new List<string>() { "Javascript", "C#", "Oracle", ".Net" }
            };
            customAttributes.Add("Skills", ca);

            -------------------------------- */

            // Set the jobs expire timestamp
            var NumSeconds = new DateTimeOffset(jobPosting.PostingExpirationDateUTC).ToUnixTimeSeconds();
            Timestamp ExpireTimestamp = new Timestamp();
            ExpireTimestamp.Seconds = NumSeconds;

            var jobToBeCreated = new Job()
            {
                RequisitionId = jobPosting.JobPostingGuid.ToString(),
                Title = jobPosting.Title,
                Description = jobPosting.Description,
                ApplicationInfo = applicationInfo,
                CompanyName = jobPosting.Company.GoogleCloudUri,
                PostingExpireTime = ExpireTimestamp.ToString(),
                Addresses = new List<string>()
                    {
                       jobPosting.Location
                    }
            };

            // Add custom attributes if they've been defined 
            if (customAttributes.Count > 0)
                jobToBeCreated.CustomAttributes = customAttributes;

            return jobToBeCreated;
        }


    }
}
