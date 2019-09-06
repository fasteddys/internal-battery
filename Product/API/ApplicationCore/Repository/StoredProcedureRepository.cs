using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data.SqlClient;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using System.Data;
using UpDiddyLib.Dto;
using UpDiddyLib.Dto.User;
namespace UpDiddyApi.ApplicationCore.Repository
{
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

        public async Task<List<JobCountPerProvince>> GetJobCountPerProvince()
        {
            return await _dbContext.JobCountPerProvince.FromSql<JobCountPerProvince>("System_JobCountPerProvince").ToListAsync();
        }


        public async Task<List<SubscriberSourceDto>> GetSubscriberSources(int SubscriberId)
        {
            var spParams = new object[] {
                   new SqlParameter("@Subscriberid", SubscriberId)
                };
            return await _dbContext.SubscriberSourcesDetails.FromSql<SubscriberSourceDto>("System_Get_SubscriberSources @SubscriberId", spParams).ToListAsync();
        }

        public async Task<List<JobDto>> GetSubscriberJobFavorites(int SubscriberId)
        {
            var spParams = new object[] {
                new SqlParameter("@SubscriberId", SubscriberId)
                };
            return await _dbContext.SubscriberJobFavorites.FromSql<JobDto>("System_Get_SubscriberJobFavorites @SubscriberId", spParams).ToListAsync();
        }

        public async Task<List<SubscriberSignUpCourseEnrollmentStatistics>> GetSubscriberSignUpCourseEnrollmentStatisticsAsync(DateTime startDate, DateTime endDate)
        {
            var spParams = new object[] {
                new SqlParameter("@StartDate", startDate),
                new SqlParameter("@EndDate", endDate)
                };
            return await _dbContext.SubscriberSignUpCourseEnrollmentStatistics.FromSql<SubscriberSignUpCourseEnrollmentStatistics>("EXECUTE dbo.System_SubscriberSignUpAndCourseEnrollmentStatisticsByPartner @StartDate, @EndDate", spParams).ToListAsync();
        }

        public async Task<int> AddOrUpdateCourseAsync(CourseParams courseParams)
        {
            var courseId = new SqlParameter("@CourseId", SqlDbType.Int);
            courseId.Direction = ParameterDirection.InputOutput;
            if (courseParams.CourseId.HasValue)
                courseId.Value = courseParams.CourseId.Value;
            else
                courseId.Value = DBNull.Value;
            var courseVariantTypeGuid = new SqlParameter("@CourseVariantTypeGuid", courseParams.CourseVariantTypeGuid);
            var vendorGuid = new SqlParameter("@VendorGuid", courseParams.VendorGuid);
            var price = new SqlParameter("@Price", SqlDbType.Decimal);
            price.Precision = 18;
            price.Scale = 2;
            price.Value = courseParams.Price;
            var code = new SqlParameter("@Code", (object)courseParams.Code ?? DBNull.Value);
            var name = new SqlParameter("@Name", courseParams.Name);
            var description = new SqlParameter("@Description", (object)courseParams.Description ?? DBNull.Value);
            var isExternal = new SqlParameter("@IsExternal", courseParams.IsExternal);
            var errorLine = new SqlParameter("@ErrorLine", SqlDbType.NVarChar, -1);
            errorLine.Direction = ParameterDirection.Output;
            var errorMessage = new SqlParameter("@ErrorMessage", SqlDbType.NVarChar, -1);
            errorMessage.Direction = ParameterDirection.Output;
            var errorProcedure = new SqlParameter("@ErrorProcedure", SqlDbType.NVarChar, -1);
            errorProcedure.Direction = ParameterDirection.Output;

            DataTable tagTable = new DataTable();
            tagTable.Columns.Add("Guid", typeof(Guid));
            if (courseParams.TagGuids != null && courseParams.TagGuids.Count > 0)
            {
                foreach (var tag in courseParams.TagGuids)
                {
                    tagTable.Rows.Add(tag);
                }
            }
            var tagGuids = new SqlParameter("@TagGuids", tagTable);
            tagGuids.SqlDbType = SqlDbType.Structured;
            tagGuids.TypeName = "dbo.GuidList";

            DataTable skillTable = new DataTable();
            skillTable.Columns.Add("Guid", typeof(Guid));
            if (courseParams.SkillGuids != null && courseParams.SkillGuids.Count > 0)
            {
                foreach (var skill in courseParams.SkillGuids)
                {
                    skillTable.Rows.Add(skill);
                }
            }
            var skillGuids = new SqlParameter("@SkillGuids", skillTable);
            skillGuids.SqlDbType = SqlDbType.Structured;
            skillGuids.TypeName = "dbo.GuidList";

            var spParams = new object[] { courseId, courseVariantTypeGuid, vendorGuid, price, code, name, description, isExternal, tagGuids, skillGuids, errorLine, errorMessage, errorProcedure };

            var rowsAffected = _dbContext.Database.ExecuteSqlCommand(@"
                EXEC [dbo].[System_Update_Course] 
                    @CourseId OUTPUT,
                    @CourseVariantTypeGuid,
	                @VendorGuid,
                    @Price,
                    @Code,
	                @Name,
                    @Description,
	                @IsExternal,
                    @TagGuids,
	                @SkillGuids,
                    @ErrorLine OUTPUT,
                    @ErrorMessage OUTPUT,
                    @ErrorProcedure OUTPUT", spParams);

            if (errorLine.Value != DBNull.Value && errorMessage.Value != DBNull.Value && errorProcedure.Value != DBNull.Value)
                throw new ApplicationException($"An error occurred in {errorProcedure.Value.ToString()} at line {errorLine.Value.ToString()}: {errorMessage.Value.ToString()}");

            return (int)courseId.Value;
        }
    }
}