using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using UpDiddyLib.Dto.User;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface IJobPostingFavoriteRepository : IUpDiddyRepositoryBase<JobPostingFavorite>
    {
        IQueryable<JobPostingFavorite> GetAllJobPostingFavoritesAsync();

    }
}