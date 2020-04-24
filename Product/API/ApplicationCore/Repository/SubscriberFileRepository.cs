using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using Microsoft.EntityFrameworkCore;
namespace UpDiddyApi.ApplicationCore.Repository
{
    public class SubscriberFileRepository : UpDiddyRepositoryBase<SubscriberFile>, ISubscriberFileRepository
    {
        private readonly UpDiddyDbContext _dbContext;
        public SubscriberFileRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
        public IQueryable<SubscriberFile> GetAllSubscriberFileQueryableAsync()
        {
            return GetAll();
        }

        public async Task UpdateSubscriberFileAsync(SubscriberFile subscriberFile)
        {
            Update(subscriberFile);
            await SaveAsync();
        }

        public async Task<List<SubscriberFile>> GetAllSubscriberFilesBySubscriberGuid(Guid subscriberGuid)
        {
            return await (from sf in _dbContext.SubscriberFile
                          join s in _dbContext.Subscriber on sf.SubscriberId equals s.SubscriberId
                          where s.SubscriberGuid == subscriberGuid && sf.IsDeleted == 0
                          select sf).ToListAsync();
        }

        public async Task<SubscriberFile> GetMostRecentBySubscriberGuid(Guid subscriberGuid)
        {
            return await (from sf in _dbContext.SubscriberFile
                          join s in _dbContext.Subscriber on sf.SubscriberId equals s.SubscriberId
                          where s.SubscriberGuid == subscriberGuid && sf.IsDeleted == 0
                          orderby sf.CreateDate descending
                          select sf).FirstOrDefaultAsync();
        }

        public async Task<SubscriberFile> GetMostRecentBySubscriberGuidForRecruiter(Guid profileGuid, Guid subscriberGuid)
        {

            return await (from p in _dbContext.Profile
                          join c in _dbContext.Company on p.CompanyId equals c.CompanyId
                          join rc in _dbContext.RecruiterCompany on c.CompanyId equals rc.CompanyId
                          join r in _dbContext.Recruiter on rc.RecruiterId equals r.RecruiterId
                          join rs in _dbContext.Subscriber on r.SubscriberId equals rs.SubscriberId
                          join s in _dbContext.Subscriber on p.SubscriberId equals s.SubscriberId
                          join sf in _dbContext.SubscriberFile on s.SubscriberId equals sf.SubscriberId
                          where p.ProfileGuid == profileGuid && rs.SubscriberGuid == subscriberGuid
                          orderby sf.CreateDate descending
                          select sf)
                          .FirstOrDefaultAsync();
        }

        public async Task<DateTime?> GetMostRecentCreatedDate(Guid subscriberGuid) => await _dbContext.SubscriberFile
            .Include(sf => sf.Subscriber)
            .Where(sf => sf.IsDeleted == 0 && sf.Subscriber.IsDeleted == 0 && sf.Subscriber.SubscriberGuid == subscriberGuid)
            .Select(sf => sf.CreateDate)
            .OrderByDescending(d => d)
            .FirstOrDefaultAsync();
    }
}
