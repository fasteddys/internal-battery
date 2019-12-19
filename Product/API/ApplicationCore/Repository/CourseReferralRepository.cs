using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using Microsoft.EntityFrameworkCore;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class CourseReferralRepository : UpDiddyRepositoryBase<CourseReferral>, ICourseReferralRepository
    {
        private readonly UpDiddyDbContext _dbContext;
        public CourseReferralRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }


        public async Task<Guid> AddCourseReferralAsync(CourseReferral courseReferral)
        {
            //persist jobReferral to database and return jobReferralGuid
            await Create(courseReferral);
            SaveAsync().Wait();

            return courseReferral.CourseReferralGuid;
        }

        public async Task<CourseReferral> GetJobReferralByGuid(Guid courseReferralGuid)
        {
            var queryableCourseReferral = GetAll();
            var courseReferralResult = await queryableCourseReferral
                                .Where(cr => cr.IsDeleted == 0 && cr.CourseReferralGuid == courseReferralGuid)
                                .ToListAsync();

            return courseReferralResult.Count == 0 ? null : courseReferralResult[0];
        }

        public async Task UpdateCourseReferral(CourseReferral courseReferral)
        {
            //persist jobReferral changes to database 
            Update(courseReferral);
            await SaveAsync();
        }

    }
}
