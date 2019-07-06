using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.Models;

namespace UpDiddyApi.Helpers
{
    public static class SendGridHelper
    {
        public static dynamic GenerateJobAbandonmentEmailTemplate(KeyValuePair<Subscriber, List<JobPosting>> pair, List<JobPosting> similarJobs, string ViewJobPostingUrl)
        {
            dynamic templateData = new JObject();
            templateData.firstName = pair.Key.FirstName;
            templateData.lastName = pair.Key.LastName;
            templateData.abandonedJobs = JArray.FromObject(pair.Value.Select(j => new
            {
                title = j.Title,
                location = $"{j.City}, {j.Province}, {j.Country}",
                posted = j.PostingDateUTC.ToShortDateString(),
                url = ViewJobPostingUrl + j.JobPostingGuid
            }).ToList());
            if (similarJobs.Count > 0)
            {
                templateData.showSimilarJobs = true;
                templateData.SimilarJobCount = similarJobs.Count;
                templateData.jobs = JArray.FromObject(similarJobs.Select(j => new
                {
                    title = j.Title,
                    location = $"{j.City}, {j.Province}, {j.Country}",
                    posted = j.PostingDateUTC.ToShortDateString(),
                    url = ViewJobPostingUrl + j.JobPostingGuid
                }).ToList());
            }
            return templateData;
        }

        public static dynamic GenerateJobAbandonmentRecruiterTemplate(Dictionary<Subscriber, List<JobPosting>> pair, string jobPostingUrl)
        {
            dynamic templateData = new JObject();
            templateData.abandonedJob = JArray.FromObject(pair.Select(j => new
            {
                subscriber = new
                {
                    firstName = j.Key.FirstName,
                    lastName =j.Key.LastName,
                    subscriberid = j.Key.SubscriberId,
                    email = j.Key.Email,
                    state = j.Key.State.Name,
                    city = j.Key.City,
                    phoneNumber = j.Key.PhoneNumber
                },
                jobAbandoned = JArray.FromObject(j.Value
                .Select(x => new
                {
                    title = x.Title,
                    postingDate = x.PostingDateUTC.ToShortDateString(),
                    jobUrl = jobPostingUrl + x.JobPostingGuid,
                    city = x.City,
                    province = x.Province,
                    country = x.Country,

                }))
                 .ToArray()
            }).ToList());
            return templateData;
        }
    }
}
