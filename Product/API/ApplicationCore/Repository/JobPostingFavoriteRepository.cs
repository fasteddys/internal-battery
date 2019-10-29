using System.ComponentModel.DataAnnotations;
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
    public class JobPostingFavoriteRepository : UpDiddyRepositoryBase<JobPostingFavorite>, IJobPostingFavoriteRepository
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

        public async Task<JobPostingFavorite> GetBySubscriberAndJobPostingGuid(Guid Subscriber, Guid job)
        {
            return await _dbContext.JobPostingFavorite.Where(x => x.Subscriber.SubscriberGuid == Subscriber && x.JobPosting.JobPostingGuid == job).FirstOrDefaultAsync();
        }

        public async Task<List<JobFavoriteDto>> GetBySubscriberGuid(Guid SubscriberGuid)
        {
            return await (from jf in _dbContext.JobPostingFavorite
                          join jp in _dbContext.JobPosting on jf.JobPostingId equals jp.JobPostingId
                          join s in _dbContext.Subscriber on jf.SubscriberId equals s.SubscriberId
                          join c in _dbContext.Company on jp.CompanyId equals c.CompanyId
                          join ja in _dbContext.JobApplication
                          on new { jf.JobPostingId, jf.SubscriberId } equals new { ja.JobPostingId, ja.SubscriberId } into tmp
                          from ja in tmp.DefaultIfEmpty()
                          where jf.IsDeleted == 0
                          select new JobFavoriteDto
                          {
                              JobPostingGuid = jp.JobPostingGuid,
                              PostingDateUTC = jp.PostingDateUTC,
                              ExpirationDateUTC = jp.PostingExpirationDateUTC,
                              HasApplied = ja == null ? false : true,
                              CompanyLogoUrl = c.LogoUrl,
                              CompanyName = c.CompanyName,
                              Title = jp.Title,
                              City = jp.City,
                              Province = jp.Province
                          }).ToListAsync();
        }
    }
}