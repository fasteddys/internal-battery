using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class JobPostingFavoriteRepository :UpDiddyRepositoryBase<JobPostingFavorite>, IJobPostingFavoriteRepository
    {
        private readonly UpDiddyDbContext _dbContext;
        public JobPostingFavoriteRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public IQueryable<JobPostingFavorite> GetAllJobPostingFavoritesAsync()
        {
           return GetAll();
        }
    }
}