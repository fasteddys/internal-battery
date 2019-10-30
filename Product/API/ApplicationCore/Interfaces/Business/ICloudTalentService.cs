using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using UpDiddyApi.Models;
using CloudTalentSolution = Google.Apis.CloudTalentSolution.v3.Data;
using UpDiddyLib.Dto;
using Google.Apis.CloudTalentSolution.v3.Data;
using UpDiddyLib.Shared.GoogleJobs;
using UpDiddyApi.Helpers.GoogleProfile;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface ICloudTalentService
    {
        BasicResponseDto ProfileTenantList();
        BasicResponseDto DeleteProfileFromCloudTalentByUri(string talentCloudUri);
        Task<bool> DeleteProfileFromCloudTalent(Guid subscriberGuid, Guid? cloudIdentifier);
        Task<bool> AddOrUpdateProfileToCloudTalent(Guid subscriberGuid);
        Task<bool> IndexProfile(Subscriber subscriber, IList<SubscriberSkill> skills);
        Task<bool> ReIndexProfile(Subscriber subscriber, IList<SubscriberSkill> skills);
        Task<bool> RemoveProfileFromIndex(Subscriber subscriber);
        SearchProfilesRequest CreateProfileSearchRequest(ProfileQueryDto profileQuery);
        ProfileSearchResultDto ProfileSearch(ProfileQueryDto profileQuery, bool isJobPostingAlertSearch = false);
        bool DeleteJobFromIndex(string googleUri);
        Task<bool> RemoveJobFromIndex(JobPosting jobPosting);
        Task<CloudTalentSolution.Job> ReIndexJob(JobPosting jobPosting);
        Task<CloudTalentSolution.Job> IndexJob(JobPosting jobPosting);
        Task<CloudTalentSolution.Company> IndexCompany(Models.Company company);
        Task<bool> AddJobToCloudTalent(Guid jobPostingGuid);
        Task<bool> UpdateJobToCloudTalent( Guid jobPostingGuid);
        Task<bool> DeleteJobFromCloudTalent( Guid jobPostingGuid);
        CloudTalentSolution.SearchJobsRequest CreateJobSearchRequest(JobQueryDto jobQuery);
        JobSearchSummaryResultDto JobSummarySearch(JobQueryDto jobQuery, bool isJobPostingAlertSearch = false);
        JobSearchResultDto JobSearch(JobQueryDto jobQuery, bool isJobPostingAlertSearch = false);
        Task<ClientEvent> CreateClientEventAsync(string requestId, ClientEventType type, List<string> jobNames, string parentEventId = null);
    }
}
