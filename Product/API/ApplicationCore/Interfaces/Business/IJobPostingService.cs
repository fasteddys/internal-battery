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
        Task<List<JobPostingCountDto>> GetJobCountPerProvinceAsync();
        Task<List<JobDto>> GetSubscriberJobFavorites(int SubscriberId);

        Task<bool> CreateJobPosting(Guid subscriberGuid, UpDiddyLib.Dto.JobPostingDto jobPostingDto);
        Task<bool> UpdateJobPosting(Guid subscriberGuid, Guid jobPostingGuid, UpDiddyLib.Dto.JobPostingDto jobPostingDto);
        Task<bool> DeleteJobPosting(Guid subscriberGuid, Guid jobPostingGuid);
        Task<UpDiddyLib.Dto.JobPostingDto> GetJobPosting(Guid subscriberGuid, Guid jobPostingGuid);

        Task<List<UpDiddyLib.Dto.JobPostingDto>> GetJobPostingForSubscriber(Guid subscriberGuid);
    }
}
