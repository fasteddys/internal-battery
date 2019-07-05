using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.Models;

namespace UpDiddyApi.Helpers
{
    public static class SendGridHelper
    {
        public static dynamic GenerateJobAbandonementEmailTemplate(KeyValuePair<Subscriber, List<JobPosting>> pair, List<JobPosting> similarJobs, string ViewJobPostingUrl)
        {
            dynamic templateData = new JObject();
            templateData.firstName = pair.Key.FirstName;
            templateData.lastName = pair.Key.LastName;
            templateData.jobTitles = JArray.FromObject(pair.Value.Select(x => x.Title).ToList());
            if (similarJobs.Count > 0)
            {
                templateData.showSimilarJobs = true;
                templateData.jobs = JArray.FromObject(similarJobs.Select(j => new
                {
                    title = j.Title,
                    summary = j.Description.Length <= 250 ? j.Description : j.Description.Substring(0, 250) + "...",
                    location = $"{j.City}, {j.Province}, {j.Country}",
                    posted = j.PostingDateUTC.ToShortDateString(),
                    url = ViewJobPostingUrl + j.JobPostingGuid
                }).ToList());
            }
            return templateData;
        }
    }
}
