using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Dto;
using Microsoft.AspNetCore.Http;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface IJobService
    {
        Task ReferJobToFriend(JobReferralDto jobReferralDto);
        Task UpdateJobReferral(string referrerCode, string subscriberGuid);
        Task UpdateJobViewed(string referrerCode);
        Task<JobSearchResultDto> GetJobsByLocationAsync(string Country, string Province, string City, string Industry, string JobCategory, string Skill, int PageNum,IQueryCollection query);
        Task<JobSearchSummaryResultDto> SummaryJobSearch(IQueryCollection query);
        Task<JobPostingAlertDto> SaveJobAlert(Guid job, Guid subscriberGuid);
    }
}
