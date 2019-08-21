using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data.SqlClient;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;


namespace UpDiddyApi.ApplicationCore.Repository
{
    using UpDiddyApi.Models;
    public class StoredProcedureRepository : IStoredProcedureRepository
    {
        private readonly UpDiddyDbContext _dbContext;
        public StoredProcedureRepository(UpDiddyDbContext dbContext) 
        {
            _dbContext = dbContext;
        }

        public async Task<List<JobAbandonmentStatistics>> GetJobAbandonmentStatisticsAsync(DateTime startDate, DateTime endDate)
        {
            var spParams = new object[] {
                new SqlParameter("@StartDate", startDate),
                new SqlParameter("@EndDate", endDate)
                };
            return await _dbContext.JobAbandonmentStatistics.FromSql<JobAbandonmentStatistics>("System_JobAbandonmentStatistics @StartDate, @EndDate", spParams).ToListAsync();
        }

        public async Task<List<JobCountPerProvince>>  GetJobCountPerProvince()
        {
            return await _dbContext.JobCountPerProvince.FromSql<JobCountPerProvince>("System_JobCountPerProvince").ToListAsync();
        }

        public async Task<List<SubscriberSignUpCourseEnrollmentStatistics>> GetSubscriberSignUpCourseEnrollmentStatisticsAsync(DateTime startDate, DateTime endDate)
        {
            var spParams = new object[] {
                new SqlParameter("@StartDate", startDate),
                new SqlParameter("@EndDate", endDate)
                };
            return await _dbContext.SubscriberSignUpCourseEnrollmentStatistics.FromSql<SubscriberSignUpCourseEnrollmentStatistics>("EXECUTE dbo.System_SubscriberSignUpAndCourseEnrollmentStatisticsByPartner @StartDate, @EndDate", spParams).ToListAsync();
        }
    }
}