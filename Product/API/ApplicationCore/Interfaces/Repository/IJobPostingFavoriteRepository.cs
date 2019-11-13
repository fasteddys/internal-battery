using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using UpDiddyLib.Dto.User;
using System;
using UpDiddyLib.Domain.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface IJobPostingFavoriteRepository : IUpDiddyRepositoryBase<JobPostingFavorite>
    {
        IQueryable<JobPostingFavorite> GetAllJobPostingFavoritesAsync();
        Task<JobPostingFavorite> GetBySubscriberAndJobPostingGuid(Guid subscriberGuid, Guid jobPostingGuid);
        Task<List<JobPostingDto>> GetBySubscriberGuid(Guid subscriberGuid);
    }
}