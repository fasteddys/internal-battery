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
        
        public async Task CacheRelatedJobSkillMatrix()
        {
            var errorLine = new SqlParameter("@ErrorLine", SqlDbType.NVarChar, -1);
            errorLine.Direction = ParameterDirection.Output;
            var errorMessage = new SqlParameter("@ErrorMessage", SqlDbType.NVarChar, -1);
            errorMessage.Direction = ParameterDirection.Output;
            var errorProcedure = new SqlParameter("@ErrorProcedure", SqlDbType.NVarChar, -1);
            errorProcedure.Direction = ParameterDirection.Output;

            var spParams = new object[] { errorLine, errorMessage, errorProcedure };

            var rowsAffected = _dbContext.Database.ExecuteSqlCommand(@"
                EXEC [dbo].[System_Cache_RelatedJobSkillMatrix]
                    @ErrorLine OUTPUT,
                    @ErrorMessage OUTPUT,
                    @ErrorProcedure OUTPUT", spParams);

            if (errorLine.Value != DBNull.Value && errorMessage.Value != DBNull.Value && errorProcedure.Value != DBNull.Value)            
                throw new ApplicationException($"An error occurred in {errorProcedure.Value.ToString()} at line {errorLine.Value.ToString()}: {errorMessage.Value.ToString()}");
        }

        public async Task<List<CourseDetailDto>> GetCoursesBySkillHistogram(Dictionary<string, int> SkillHistogram, int NumCourses)
        {
            List<CourseDetailDto> rval = null;

            DataTable table = new DataTable();
            table.Columns.Add("Skill", typeof(string));
            table.Columns.Add("Occurences", typeof(int));

            foreach (KeyValuePair<string, int> SkillInfo in SkillHistogram)
            {
                DataRow row = table.NewRow();
                row["Skill"] = SkillInfo.Key;
                row["Occurences"] = SkillInfo.Value;
                table.Rows.Add(row);
            }


            var Skills = new SqlParameter("@SkillHistogram", table);
            Skills.SqlDbType = SqlDbType.Structured;
            Skills.TypeName = "dbo.SkillHistogram";

            var spParams = new object[] { Skills, new SqlParameter("@MaxResults", NumCourses) };
            rval = await _dbContext.CourseDetails.FromSql<CourseDetailDto>("System_Get_RelatedCoursesBySkills @SkillHistogram,@MaxResults", spParams).ToListAsync();

            return rval;
        }



        public async Task<List<CourseDetailDto>> GetCoursesRandom(int NumCourses)
        {
            var spParams = new object[] {
                new SqlParameter("@MaxResults", NumCourses)
                };

            List<CourseDetailDto> rval = null;
            rval = await _dbContext.CourseDetails.FromSql<CourseDetailDto>("System_Get_CoursesRandom @MaxResults", spParams).ToListAsync();
            return rval;
        }



        public async Task<List<CourseDetailDto>> GetCoursesForJob(Guid JobGuid, int NumCourses)
        {
            var spParams = new object[] {
                new SqlParameter("@JobGuid", JobGuid),
                new SqlParameter("@MaxResults", NumCourses)
                };

            List<CourseDetailDto> rval = null;
            rval = await _dbContext.CourseDetails.FromSql<CourseDetailDto>("System_Get_CoursesForJob @JobGuid,@MaxResults", spParams).ToListAsync();
            return rval;
        }




        public async Task<List<SearchTermDto>> GetKeywordSearchTermsAsync()
        {
            return await _dbContext.KeywordSearchTerms.FromSql<SearchTermDto>("System_Get_KeywordSearchTerms").ToListAsync();
        }

        public async Task<List<SearchTermDto>> GetLocationSearchTermsAsync()
        {
            return await _dbContext.LocationSearchTerms.FromSql<SearchTermDto>("System_Get_LocationSearchTerms").ToListAsync();
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

        public async Task<List<SubscriberInitialSourceDto>> GetNewSubscribers()
        {
            List<SubscriberInitialSourceDto> rval = null;
            rval = await _dbContext.SubscriberInitialSource.FromSql<SubscriberInitialSourceDto>("System_Get_New_Subscribers").ToListAsync();
            return rval;
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

        public async Task<List<SubscriberSignUpCourseEnrollmentStatistics>> GetSubscriberSignUpCourseEnrollmentStatisticsAsync(DateTime? startDate, DateTime? endDate)
        {
            var spParams = new object[] {
                new SqlParameter("@StartDate", startDate.HasValue ? startDate.Value : (object)DBNull.Value),
                new SqlParameter("@EndDate", endDate.HasValue ? endDate.Value : (object)DBNull.Value),
                };
            return await _dbContext.SubscriberSignUpCourseEnrollmentStatistics.FromSql<SubscriberSignUpCourseEnrollmentStatistics>("EXECUTE dbo.System_Get_SubscriberSignUpCourseEnrollmentStatistics @StartDate, @EndDate", spParams).ToListAsync();
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

        public async Task<List<CourseDetailDto>> GetCourses(int limit, int offset, string sort, string order)
        {
            var spParams = new object[] {
                new SqlParameter("@Limit", limit),
                new SqlParameter("@Offset", offset),
                new SqlParameter("@Sort", sort),
                new SqlParameter("@Order", order),
                };

            List<CourseDetailDto> rval = null;
            rval = await _dbContext.CourseDetails.FromSql<CourseDetailDto>("System_Get_Courses @Limit, @Offset, @Sort, @Order", spParams).ToListAsync();
            return rval;
        }

        public async Task<CourseDetailDto> GetCourse(Guid courseGuid)
        {
            var spParams = new object[] {
                new SqlParameter("@CourseGuid", courseGuid)
                };

            CourseDetailDto rval = null;
            rval = await _dbContext.CourseDetails.FromSql<CourseDetailDto>("System_Get_Course @CourseGuid", spParams).FirstOrDefaultAsync();
            return rval;
        }

        public async Task<List<CourseDetailDto>> GetFavoriteCourses(Guid subscriberGuid, int limit, int offset, string sort, string order)
        {
            var spParams = new object[] {
                new SqlParameter("@SubscriberGuid", subscriberGuid),
                new SqlParameter("@Limit", limit),
                new SqlParameter("@Offset", offset),
                new SqlParameter("@Sort", sort),
                new SqlParameter("@Order", order),
                };

            List<CourseDetailDto> rval = null;
            rval = await _dbContext.CourseDetails.FromSql<CourseDetailDto>("System_Get_Favorite_Courses @SubscriberGuid, @Limit, @Offset, @Sort, @Order", spParams).ToListAsync();
            return rval;
        }



        public async Task<List<SubscriberNotesDto>> GetSubscriberNotes(Guid subscriberGuid, Guid talentGuid, int limit, int offset, string sort, string order)
        {
            var spParams = new object[] {
                new SqlParameter("@SubscriberGuid", subscriberGuid),
                new SqlParameter("@TalentGuid", talentGuid),
                new SqlParameter("@Limit", limit),
                new SqlParameter("@Offset", offset),
                new SqlParameter("@Sort", sort),
                new SqlParameter("@Order", order),
                };

            List<SubscriberNotesDto> rval = null;
            rval = await _dbContext.SubscriberNoteQuery.FromSql<SubscriberNotesDto>("System_Get_SubscriberNotes @SubscriberGuid, @TalentGuid, @Limit, @Offset, @Sort, @Order", spParams).ToListAsync();
            return rval;
        }

 
    }
}