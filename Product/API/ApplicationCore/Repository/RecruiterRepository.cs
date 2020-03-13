using System;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using Microsoft.EntityFrameworkCore;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class RecruiterRepository : UpDiddyRepositoryBase<Recruiter>, IRecruiterRepository
    {
        public RecruiterRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {

        }

        public async Task AddRecruiter(Recruiter recruiter)
        {
            await Create(recruiter);
            await SaveAsync();
        }

        public IQueryable<Recruiter> GetAllRecruiters()
        {
            return GetAll();
        }

        public async Task<Recruiter> GetRecruiterByRecruiterGuid(Guid recruiterGuid)
        {
            var queryableRecruiter = GetAll();
            var recruiterResult = await queryableRecruiter
                                .Where(jr => jr.IsDeleted == 0 && jr.RecruiterGuid == recruiterGuid)
                                .ToListAsync();

            return recruiterResult.Count == 0 ? null : recruiterResult[0];
        }

        public async Task<Recruiter> GetRecruiterBySubscriberId(int subscriberId)
        {
            var queryableRecruiter =  GetAll();
            var recruiterResult = await queryableRecruiter
                                .Where(jr => jr.IsDeleted == 0 && jr.SubscriberId == subscriberId)
                                .ToListAsync();

            return recruiterResult.Count == 0 ? null : recruiterResult[0];
        }


        public async Task<Recruiter> GetRecruiterBySubscriberGuid(Guid subscriberGuid)
        {
            var queryableRecruiter = GetAll();
            var recruiterResult = await queryableRecruiter
                                .Where(jr => jr.IsDeleted == 0 && jr.Subscriber.SubscriberGuid == subscriberGuid)
                                .ToListAsync();

            return recruiterResult.Count == 0 ? null : recruiterResult[0];
        }

        public async Task<Recruiter> GetRecruiterAndCompanyBySubscriberGuid(Guid subscriberGuid)
        {
            var queryableRecruiter = GetAll();
            var recruiterResult = await queryableRecruiter
                                .Include( c => c.Company)
                                .Where(jr => jr.IsDeleted == 0 && jr.Subscriber.SubscriberGuid == subscriberGuid)
                                .ToListAsync();

            return recruiterResult.Count == 0 ? null : recruiterResult[0];
        }


        


        public async Task UpdateRecruiter(Recruiter recruiter)
        {
            Update(recruiter);
            await SaveAsync();
        }
    }
}
