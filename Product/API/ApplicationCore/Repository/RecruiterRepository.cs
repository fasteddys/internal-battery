using System;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class RecruiterRepository : UpDiddyRepositoryBase<Recruiter>, IRecruiterRepository
    {
        private UpDiddyDbContext _dbContext;
        public RecruiterRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddRecruiter(Recruiter recruiter)
        {
            await Create(recruiter);
            await SaveAsync();
        }

        public async Task<List<Recruiter>> GetAllInternalRecruiters()
        {
            return await (from r in _dbContext.Recruiter.Include(r => r.Subscriber).Include(r => r.RecruiterCompanies).ThenInclude(rc => rc.Company)
                          where r.Subscriber != null && r.RecruiterCompanies.Any() && r.IsDeleted == 0
                          select r).ToListAsync();
        }

        public async Task<Recruiter> GetRecruiterByRecruiterGuid(Guid recruiterGuid)
        {
            return await (from r in _dbContext.Recruiter.Include(r => r.Subscriber).Include(r => r.RecruiterCompanies).ThenInclude(rc => rc.Company)
                          where r.RecruiterCompanies.Any() && r.IsDeleted == 0 && r.RecruiterGuid == recruiterGuid
                          select r).FirstOrDefaultAsync();
        }

        public async Task<Recruiter> GetRecruiterBySubscriberId(int subscriberId)
        {
            return await (from r in _dbContext.Recruiter.Include(r => r.Subscriber).Include(r => r.RecruiterCompanies).ThenInclude(rc => rc.Company)
                          where r.RecruiterCompanies.Any() && r.IsDeleted == 0 && r.SubscriberId == subscriberId
                          select r).FirstOrDefaultAsync();
        }


        public async Task<Recruiter> GetRecruiterBySubscriberGuid(Guid subscriberGuid)
        {
            return await (from r in _dbContext.Recruiter.Include(r => r.Subscriber).Include(r => r.RecruiterCompanies).ThenInclude(rc => rc.Company)
                          where r.RecruiterCompanies.Any() && r.IsDeleted == 0 && r.Subscriber.SubscriberGuid == subscriberGuid
                          select r).FirstOrDefaultAsync();
        }

        public async Task<Recruiter> GetRecruiterAndCompanyBySubscriberGuid(Guid subscriberGuid)
        {
            return await (from r in _dbContext.Recruiter.Include(r => r.Subscriber).Include(r => r.RecruiterCompanies).ThenInclude(rc => rc.Company)
                          where r.RecruiterCompanies.Any() && r.IsDeleted == 0 && r.Subscriber.SubscriberGuid == subscriberGuid
                          select r).FirstOrDefaultAsync();
        }

        public async Task UpdateRecruiter(Recruiter recruiter)
        {
            Update(recruiter);
            await SaveAsync();
        }
    }
}
