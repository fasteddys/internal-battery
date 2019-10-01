using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyLib.Dto.User;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class JobPostingFavoriteRepository :UpDiddyRepositoryBase<JobPostingFavorite>, IJobPostingFavoriteRepository
    {
        private readonly UpDiddyDbContext _dbContext;
        public JobPostingFavoriteRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<JobDto>> GetSubscriberJobFavorites(int SubscriberId)
        {
            var spParams = new object[] {
                new SqlParameter("@SubscriberId", SubscriberId) 
                };
            return await _dbContext.SubscriberJobFavorites.FromSql<JobDto>("System_Get_SubscriberJobFavorites @SubscriberId", spParams).ToListAsync();
        }

        public IQueryable<JobPostingFavorite> GetAllJobPostingFavoritesAsync()
        {
           return GetAll();
        }
    }
}