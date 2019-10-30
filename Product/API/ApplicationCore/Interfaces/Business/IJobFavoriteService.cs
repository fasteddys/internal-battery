using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using UpDiddyLib.Domain.Models;
namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface IJobFavoriteService
    {
        Task AddJobToFavorite(Guid subscriberGuid, Guid jobPostingGuid);
        Task DeleteJobFavorite(Guid subscriberGuid, Guid jobPostingGuid);
        Task<List<JobPostingDto>> GetJobFavorites(Guid subscriberGuid);
    }
}
