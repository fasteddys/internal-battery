using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UpDiddyLib.Domain.Models;
using UpDiddyLib.Dto;
using UpDiddyLib.Dto.User;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface IJobPostingService
    {
        Task<List<RelatedJobDto>> GetJobsByCourse(Guid courseGuid, int limit, int offset, Guid? subscriberGuid = null);
        Task<List<RelatedJobDto>> GetJobsByCourses(List<Guid> courseGuids, int limit, int offset, Guid? subscriberGuid = null);
        Task<List<CareerPathJobDto>> GetCareerPathRecommendations(int limit, int offset, Guid subscriberGuid);
        Task<List<RelatedJobDto>> GetJobsBySubscriber(Guid subscriberGuid, int limit, int offset);
        Task<List<JobPostingCountDto>> GetJobCountPerProvinceAsync();
        Task<List<JobDto>> GetSubscriberJobFavorites(int SubscriberId);
        Task<Guid> CreateJobPosting(Guid subscriberGuid, JobCrudDto jobPostingDto);
        Task<bool> UpdateJobPosting(Guid subscriberGuid, Guid jobPostingGuid, JobCrudDto jobPostingDto);
        Task<bool> DeleteJobPosting(Guid subscriberGuid, Guid jobPostingGuid);
        Task<UpDiddyLib.Dto.JobPostingDto> GetJobPosting(Guid subscriberGuid, Guid jobPostingGuid);
        Task<List<UpDiddyLib.Dto.JobPostingDto>> GetJobPostingForSubscriber(Guid subscriberGuid);
        Task<JobCrudListDto> GetJobPostingCrudForSubscriber(Guid subscriberGuid, int limit, int offset, string sort, string order);
        Task<JobCrudDto> GetJobPostingCrud(Guid subscriberGuid, Guid jobPostingGuid);
        Task<bool> UpdateJobPostingSkills(Guid subscriberGuid, Guid jobPostingGuid, List<UpDiddyLib.Domain.Models.SkillDto> skills);
        Task<bool> UpdateJobPostingSkills(int jobPostingId, List<UpDiddyLib.Domain.Models.SkillDto> jobPostingSkills);
        Task<JobSiteScrapeStatisticsListDto> GetJobSiteScrapeStatistics(int limit, int offset, string sort, string order);
    }
}