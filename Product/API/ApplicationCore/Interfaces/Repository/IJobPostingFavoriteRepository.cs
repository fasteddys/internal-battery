using System.Linq;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface IJobPostingFavoriteRepository : IUpDiddyRepositoryBase<JobPostingFavorite>
    {
        IQueryable<JobPostingFavorite> GetAllJobPostingFavoritesAsync();
    }
}