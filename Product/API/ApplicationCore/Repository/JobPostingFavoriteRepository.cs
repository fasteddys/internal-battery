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
using UpDiddyLib.Helpers;
using UpDiddyLib.Domain.Models;
using Microsoft.Extensions.Configuration;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class JobPostingFavoriteRepository : UpDiddyRepositoryBase<JobPostingFavorite>, IJobPostingFavoriteRepository
    {
        private readonly UpDiddyDbContext _dbContext;
        private readonly IConfiguration _configuration;
        public JobPostingFavoriteRepository(UpDiddyDbContext dbContext, IConfiguration configuration) : base(dbContext)
        {
            _dbContext = dbContext;
            _configuration = configuration;
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

        public async Task<JobPostingFavorite> GetBySubscriberAndJobPostingGuid(Guid subscriberGuid, Guid jobPostingGuid)
        {
            return await (from jf in _dbContext.JobPostingFavorite
                          join jp in _dbContext.JobPosting on jf.JobPostingId equals jp.JobPostingId
                          join s in _dbContext.Subscriber on jf.SubscriberId equals s.SubscriberId
                          where s.SubscriberGuid == subscriberGuid && jp.JobPostingGuid == jobPostingGuid
                          select jf).FirstOrDefaultAsync();
        }

        public async Task<List<JobPostingDto>> GetBySubscriberGuid(Guid subscriberGuid)
        {
            return await (from jf in _dbContext.JobPostingFavorite
                          join jp in _dbContext.JobPosting on jf.JobPostingId equals jp.JobPostingId
                          join s in _dbContext.Subscriber on jf.SubscriberId equals s.SubscriberId
                          join c in _dbContext.Company on jp.CompanyId equals c.CompanyId
                          join ja in _dbContext.JobApplication
                          on new { jf.JobPostingId, jf.SubscriberId } equals new { ja.JobPostingId, ja.SubscriberId } into tmp
                          from ja in tmp.DefaultIfEmpty()
                          where jf.IsDeleted == 0 && s.SubscriberGuid == subscriberGuid
                          select new JobPostingDto
                          {
                              JobPostingGuid = jp.JobPostingGuid,
                              PostingDateUTC = jp.PostingDateUTC,
                              ExpirationDateUTC = jp.PostingExpirationDateUTC,
                              HasApplied = ja == null ? false : true,
                              CompanyName = c.CompanyName,
                              Title = jp.Title,
                              City = jp.City,
                              Province = jp.Province,
                              CompanyLogoUrl = _configuration["StorageAccount:AssetBaseUrl"] + "Company/" + c.LogoUrl,
                              SemanticJobPath = Utils.CreateSemanticJobPath(
                                                jp.Industry == null ? null : jp.Industry.Name,
                                                jp.JobCategory == null ? null : jp.JobCategory.Name,
                                                jp.Country,
                                                jp.Province,
                                                jp.City,
                                                jp.JobPostingGuid.ToString())
                          }).ToListAsync();
        }
    }
}