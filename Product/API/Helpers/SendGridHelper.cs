using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyLib.Helpers;
using com.traitify.net.TraitifyLibrary;

namespace UpDiddyApi.Helpers
{
    public static class SendGridHelper
    {
        public static dynamic GenerateJobAbandonmentEmailTemplate(KeyValuePair<Subscriber, List<JobPosting>> pair, List<JobViewDto> similarJobs, string ViewJobPostingUrl)
        {
            dynamic templateData = new JObject();
            templateData.firstName = pair.Key.FirstName;
            templateData.lastName = pair.Key.LastName;
            templateData.abandonedJobs = JArray.FromObject(pair.Value.Select(j => new
            {
                title = j.Title,
                summary = FormatJobSummary(j.Description),
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
                    summary = FormatJobSummary(j.Description),
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
                    lastName = j.Key.LastName,
                    subscriberid = j.Key.SubscriberId,
                    email = j.Key.Email,
                    state = j.Key.State != null ? j.Key.State.Name ?? string.Empty : string.Empty,
                    city = j.Key.City ?? string.Empty,
                    phoneNumber = j.Key.PhoneNumber ?? string.Empty,
                },
                jobAbandoned = JArray.FromObject(j.Value
                .Select(x => new
                {
                    title = x.Title,
                    summary = FormatJobSummary(x.Description),
                    postingDate = x.PostingDateUTC.ToShortDateString(),
                    jobUrl = jobPostingUrl + x.JobPostingGuid,
                    city = x.City ?? string.Empty,
                    province = x.Province ?? string.Empty,
                    country = x.Country ?? string.Empty,

                })).ToArray()
            }).ToList());
            return templateData;
        }

        private static string FormatJobSummary(string summary)
        {
            summary = Utils.RemoveHTML(summary);
            return summary.Length <= 250 ? summary : summary.Substring(0, 250) + "...";
        }

        public static dynamic GenerateTraitifyTemplate(AssessmentPersonalityTypes types)
        {
            dynamic templateData = new JObject();
            templateData.personalityBlend = types.personality_blend;
            return templateData;
        }
    }
}
