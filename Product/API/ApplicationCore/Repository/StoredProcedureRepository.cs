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
using UpDiddyLib.Domain.Models;
using UpDiddyLib.Domain.Models.Reports;
using UpDiddyLib.Domain.AzureSearchDocuments;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class StoredProcedureRepository : IStoredProcedureRepository
    {
        private readonly UpDiddyDbContext _dbContext;

        public StoredProcedureRepository(UpDiddyDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<JobSitemapDto>> GetJobSitemapUrls(Uri basesiteUrl)
        {
            List<JobSitemapDto> rval = null;
            var baseSiteUrlParam = new SqlParameter("@BaseSiteUrl", SqlDbType.VarChar, 500);
            baseSiteUrlParam.Value = basesiteUrl.ToString();
            var spParams = new object[] { baseSiteUrlParam };
            rval = await _dbContext.JobSitemap.FromSql<JobSitemapDto>("System_Get_JobSitemapUrls @BaseSiteUrl", spParams).ToListAsync();
            return rval;
        }

        public async Task<List<RelatedCourseDto>> GetCoursesByCourses(List<Guid> courseGuids, int limit, int offset)
        {
            DataTable coursesTable = new DataTable();
            coursesTable.Columns.Add("Guid", typeof(Guid));
            if (courseGuids != null && courseGuids.Count > 0)
            {
                foreach (var courseGuid in courseGuids)
                {
                    coursesTable.Rows.Add(courseGuid);
                }
            }
            var coursesParam = new SqlParameter("@CourseGuids", coursesTable);
            coursesParam.SqlDbType = SqlDbType.Structured;
            coursesParam.TypeName = "dbo.GuidList";

            var limitParam = new SqlParameter("@Limit", SqlDbType.Int);
            limitParam.Value = limit;

            var offsetParam = new SqlParameter("@Offset", SqlDbType.Int);
            offsetParam.Value = offset;

            var spParams = new object[] { coursesParam, limitParam, offsetParam };

            return await _dbContext.RelatedCourses.FromSql<RelatedCourseDto>("System_Get_CoursesByCourses @CourseGuids, @Limit, @Offset", spParams).ToListAsync();
        }

        public async Task<List<RelatedCourseDto>> GetCoursesByJobs(List<Guid> jobPostingGuids, int limit, int offset)
        {
            DataTable jobPostingsTable = new DataTable();
            jobPostingsTable.Columns.Add("Guid", typeof(Guid));
            if (jobPostingGuids != null && jobPostingGuids.Count > 0)
            {
                foreach (var jobPostingGuid in jobPostingGuids)
                {
                    jobPostingsTable.Rows.Add(jobPostingGuid);
                }
            }
            var jobPostingsParam = new SqlParameter("@JobPostingGuids", jobPostingsTable);
            jobPostingsParam.SqlDbType = SqlDbType.Structured;
            jobPostingsParam.TypeName = "dbo.GuidList";

            var limitParam = new SqlParameter("@Limit", SqlDbType.Int);
            limitParam.Value = limit;

            var offsetParam = new SqlParameter("@Offset", SqlDbType.Int);
            offsetParam.Value = offset;

            var spParams = new object[] { jobPostingsParam, limitParam, offsetParam };

            return await _dbContext.RelatedCourses.FromSql<RelatedCourseDto>("System_Get_CoursesByJobs @JobPostingGuids, @Limit, @Offset", spParams).ToListAsync();
        }

        public async Task<List<RelatedCourseDto>> GetCoursesByCourse(Guid courseGuid, int limit, int offset)
        {
            var courseParam = new SqlParameter("@CourseGuid", SqlDbType.UniqueIdentifier);
            courseParam.Value = courseGuid;

            var limitParam = new SqlParameter("@Limit", SqlDbType.Int);
            limitParam.Value = limit;

            var offsetParam = new SqlParameter("@Offset", SqlDbType.Int);
            offsetParam.Value = offset;

            var spParams = new object[] { courseParam, limitParam, offsetParam };

            return await _dbContext.RelatedCourses.FromSql<RelatedCourseDto>("System_Get_CoursesByCourse @CourseGuid, @Limit, @Offset", spParams).ToListAsync();
        }

        public async Task<List<RelatedCourseDto>> GetCoursesByJob(Guid jobPostingGuid, int limit, int offset)
        {
            var jobPostingParam = new SqlParameter("@JobPostingGuid", SqlDbType.UniqueIdentifier);
            jobPostingParam.Value = jobPostingGuid;

            var limitParam = new SqlParameter("@Limit", SqlDbType.Int);
            limitParam.Value = limit;

            var offsetParam = new SqlParameter("@Offset", SqlDbType.Int);
            offsetParam.Value = offset;

            var spParams = new object[] { jobPostingParam, limitParam, offsetParam };

            return await _dbContext.RelatedCourses.FromSql<RelatedCourseDto>("System_Get_CoursesByJob @JobPostingGuid, @Limit, @Offset", spParams).ToListAsync();
        }

        public async Task<List<RelatedCourseDto>> GetCoursesBySubscriber(Guid subscriberGuid, int limit, int offset)
        {
            var subscriberParam = new SqlParameter("@SubscriberGuid", SqlDbType.UniqueIdentifier);
            subscriberParam.Value = subscriberGuid;

            var limitParam = new SqlParameter("@Limit", SqlDbType.Int);
            limitParam.Value = limit;

            var offsetParam = new SqlParameter("@Offset", SqlDbType.Int);
            offsetParam.Value = offset;

            var spParams = new object[] { subscriberParam, limitParam, offsetParam };

            return await _dbContext.RelatedCourses.FromSql<RelatedCourseDto>("System_Get_CoursesBySubscriber @SubscriberGuid, @Limit, @Offset", spParams).ToListAsync();

        }

        public async Task<List<RelatedJobDto>> GetJobsByCourses(List<Guid> courseGuids, int limit, int offset, Guid? subscriberGuid = null)
        {
            DataTable coursesTable = new DataTable();
            coursesTable.Columns.Add("Guid", typeof(Guid));
            if (courseGuids != null && courseGuids.Count > 0)
            {
                foreach (var courseGuid in courseGuids)
                {
                    coursesTable.Rows.Add(courseGuid);
                }
            }
            var coursesParam = new SqlParameter("@CourseGuids", coursesTable);
            coursesParam.SqlDbType = SqlDbType.Structured;
            coursesParam.TypeName = "dbo.GuidList";

            var subscriberParam = new SqlParameter("@SubscriberGuid", SqlDbType.UniqueIdentifier);
            subscriberParam.Value = subscriberGuid.HasValue ? (object)subscriberGuid.Value : DBNull.Value;

            var limitParam = new SqlParameter("@Limit", SqlDbType.Int);
            limitParam.Value = limit;

            var offsetParam = new SqlParameter("@Offset", SqlDbType.Int);
            offsetParam.Value = offset;

            var spParams = new object[] { coursesParam, subscriberParam, limitParam, offsetParam };

            return await _dbContext.RelatedJobs.FromSql<RelatedJobDto>("System_Get_JobsByCourses @CourseGuids, @SubscriberGuid, @Limit, @Offset", spParams).ToListAsync();
        }

        public async Task<List<RelatedJobDto>> GetJobsByCourse(Guid courseGuid, int limit, int offset, Guid? subscriberGuid = null)
        {
            var courseParam = new SqlParameter("@CourseGuid", SqlDbType.UniqueIdentifier);
            courseParam.Value = courseGuid;

            var subscriberParam = new SqlParameter("@SubscriberGuid", SqlDbType.UniqueIdentifier);
            subscriberParam.Value = subscriberGuid.HasValue ? (object)subscriberGuid.Value : DBNull.Value;

            var limitParam = new SqlParameter("@Limit", SqlDbType.Int);
            limitParam.Value = limit;

            var offsetParam = new SqlParameter("@Offset", SqlDbType.Int);
            offsetParam.Value = offset;

            var spParams = new object[] { courseParam, subscriberParam, limitParam, offsetParam };

            return await _dbContext.RelatedJobs.FromSql<RelatedJobDto>("System_Get_JobsByCourse @CourseGuid, @SubscriberGuid, @Limit, @Offset", spParams).ToListAsync();
        }

        public async Task<List<RelatedJobDto>> GetJobsByTopic(Guid topicGuid, int limit, int offset, Guid? subscriberGuid = null)
        {
            var topicParam = new SqlParameter("@TopicGuid", SqlDbType.UniqueIdentifier);
            topicParam.Value = topicGuid;

            var subscriberParam = new SqlParameter("@SubscriberGuid", SqlDbType.UniqueIdentifier);
            subscriberParam.Value = subscriberGuid.HasValue ? (object)subscriberGuid.Value : DBNull.Value;

            var limitParam = new SqlParameter("@Limit", SqlDbType.Int);
            limitParam.Value = limit;

            var offsetParam = new SqlParameter("@Offset", SqlDbType.Int);
            offsetParam.Value = offset;

            var spParams = new object[] { topicParam, subscriberParam, limitParam, offsetParam };

            return await _dbContext.RelatedJobs.FromSql<RelatedJobDto>("System_Get_JobsByTopic @TopicGuid, @SubscriberGuid, @Limit, @Offset", spParams).ToListAsync();
        }

        public async Task<List<RelatedJobDto>> GetJobsBySubscriber(Guid subscriberGuid, int limit, int offset)
        {
            var subscriberParam = new SqlParameter("@SubscriberGuid", SqlDbType.UniqueIdentifier);
            subscriberParam.Value = subscriberGuid;

            var limitParam = new SqlParameter("@Limit", SqlDbType.Int);
            limitParam.Value = limit;

            var offsetParam = new SqlParameter("@Offset", SqlDbType.Int);
            offsetParam.Value = offset;

            var spParams = new object[] { subscriberParam, limitParam, offsetParam };

            return await _dbContext.RelatedJobs.FromSql<RelatedJobDto>("System_Get_JobsBySubscriber @SubscriberGuid, @Limit, @Offset", spParams).ToListAsync();
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

        public async Task<List<CourseDetailDto>> GetCoursesRandom(int NumCourses)
        {
            var spParams = new object[] {
                new SqlParameter("@MaxResults", NumCourses)
                };

            List<CourseDetailDto> rval = null;
            rval = await _dbContext.CourseDetails.FromSql<CourseDetailDto>("System_Get_CoursesRandom @MaxResults", spParams).ToListAsync();
            return rval;
        }

        public async Task<List<SearchTermDto>> GetKeywordSearchTermsAsync()
        {
            return await _dbContext.KeywordSearchTerms.FromSql<SearchTermDto>("System_Get_KeywordSearchTerms").ToListAsync();
        }

        public async Task<List<SearchTermDto>> GetLocationdSearchTermsAsync()
        {
            return await _dbContext.KeywordSearchTerms.FromSql<SearchTermDto>("System_Get_LocationSearchTerms").ToListAsync();
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

        public async Task<List<CourseVariantDetailDto>> GetCourseVariants(Guid courseGuid)
        {
            var spParams = new object[] {
                new SqlParameter("@CourseGuid", courseGuid)
                };

            List<CourseVariantDetailDto> rval = null;
            rval = await _dbContext.CourseVariants.FromSql<CourseVariantDetailDto>("System_Get_CourseVariants @CourseGuid", spParams).ToListAsync();
            return rval;
        }

        public async Task<List<CourseFavoriteDto>> GetFavoriteCourses(Guid subscriberGuid, int limit, int offset, string sort, string order)
        {
            var spParams = new object[] {
                new SqlParameter("@SubscriberGuid", subscriberGuid),
                new SqlParameter("@Limit", limit),
                new SqlParameter("@Offset", offset),
                new SqlParameter("@Sort", sort),
                new SqlParameter("@Order", order),
                };

            List<CourseFavoriteDto> rval = null;
            rval = await _dbContext.CourseFavorites.FromSql<CourseFavoriteDto>("System_Get_Favorite_Courses @SubscriberGuid, @Limit, @Offset, @Sort, @Order", spParams).ToListAsync();
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

        public async Task<List<SubscriberCourseDto>> GetSubscriberCourses(Guid subscriberGuid, int excludeCompleted, int excludeActive)

        {
            var spParams = new object[] {
                new SqlParameter("@SubscriberGuid", subscriberGuid),
                new SqlParameter("@ExcludeCompleted", excludeCompleted),
                new SqlParameter("@ExcludeActive", excludeActive)
                };

            List<SubscriberCourseDto> rval = null;
            rval = await _dbContext.SubscriberCourses.FromSql<SubscriberCourseDto>("System_Get_SubscriberCourses @SubscriberGuid, @ExcludeCompleted, @ExcludeActive", spParams).ToListAsync();
            return rval;
        }

        public async Task<List<UpDiddyLib.Dto.NotificationDto>> GetLegacyNotifications(int limit, int offset, string sort, string order)
        {
            var spParams = new object[] {
                new SqlParameter("@Limit", limit),
                new SqlParameter("@Offset", offset),
                new SqlParameter("@Sort", sort),
                new SqlParameter("@Order", order),
                };

            List<UpDiddyLib.Dto.NotificationDto> rval = null;
            rval = await _dbContext.LegacyNotifications.FromSql<UpDiddyLib.Dto.NotificationDto>("System_Get_LegacyNotifications  @Limit, @Offset, @Sort, @Order", spParams).ToListAsync();
            return rval;
        }

        public async Task<List<UpDiddyLib.Dto.NotificationDto>> GetLegacySubscriberNotifications(Guid subscriberGuid, int limit, int offset, string sort, string order)
        {
            var spParams = new object[] {
                new SqlParameter("@SubscriberGuid", subscriberGuid),
                new SqlParameter("@Limit", limit),
                new SqlParameter("@Offset", offset),
                new SqlParameter("@Sort", sort),
                new SqlParameter("@Order", order),
                };

            List<UpDiddyLib.Dto.NotificationDto> rval = null;
            rval = await _dbContext.LegacyNotifications.FromSql<UpDiddyLib.Dto.NotificationDto>("System_Get_LegacySubscriberNotifications @SubscriberGuid, @Limit, @Offset, @Sort, @Order", spParams).ToListAsync();
            return rval;
        }

        public async Task<List<UpDiddyLib.Domain.Models.NotificationDto>> GetNotifications(int limit, int offset, string sort, string order)
        {
            var spParams = new object[] {
                new SqlParameter("@Limit", limit),
                new SqlParameter("@Offset", offset),
                new SqlParameter("@Sort", sort),
                new SqlParameter("@Order", order),
                };

            List<UpDiddyLib.Domain.Models.NotificationDto> rval = null;
            rval = await _dbContext.Notifications.FromSql<UpDiddyLib.Domain.Models.NotificationDto>("System_Get_Notifications  @Limit, @Offset, @Sort, @Order", spParams).ToListAsync();
            return rval;
        }

        public async Task<List<UpDiddyLib.Domain.Models.NotificationDto>> GetSubscriberNotifications(Guid subscriberGuid, int limit, int offset, string sort, string order)
        {
            var spParams = new object[] {
                new SqlParameter("@SubscriberGuid", subscriberGuid),
                new SqlParameter("@Limit", limit),
                new SqlParameter("@Offset", offset),
                new SqlParameter("@Sort", sort),
                new SqlParameter("@Order", order),
                };

            List<UpDiddyLib.Domain.Models.NotificationDto> rval = null;
            rval = await _dbContext.Notifications.FromSql<UpDiddyLib.Domain.Models.NotificationDto>("System_Get_SubscriberNotifications @SubscriberGuid, @Limit, @Offset, @Sort, @Order", spParams).ToListAsync();
            return rval;
        }

        public async Task<List<CourseDetailDto>> GetCoursesByTopic(Guid topic, int limit, int offset, string sort, string order)
        {
            var spParams = new object[] {
                new SqlParameter("@Topic", topic),
                new SqlParameter("@Limit", limit),
                new SqlParameter("@Offset", offset),
                new SqlParameter("@Sort", sort),
                new SqlParameter("@Order", order),
                };

            List<CourseDetailDto> rval = null;
            rval = await _dbContext.CourseDetails.FromSql<CourseDetailDto>("System_Get_CoursesByTopic @topic, @Limit, @Offset, @Sort, @Order", spParams).ToListAsync();
            return rval;
        }

        public async Task<List<CompanyDto>> GetCompanies(int limit, int offset, string sort, string order)
        {
            var spParams = new object[] {
                new SqlParameter("@Limit", limit),
                new SqlParameter("@Offset", offset),
                new SqlParameter("@Sort", sort),
                new SqlParameter("@Order", order),
                };

            List<CompanyDto> rval = null;
            rval = await _dbContext.Companies.FromSql<CompanyDto>("System_Get_Companies @Limit, @Offset, @Sort, @Order", spParams).ToListAsync();
            return rval;
        }

        public async Task<List<JobCrudDto>> GetSubscriberJobPostingCruds(Guid subscriberGuid, int limit, int offset, string sort, string order)
        {
            var spParams = new object[] {
                new SqlParameter("@SubscriberGuid", subscriberGuid),
                new SqlParameter("@Limit", limit),
                new SqlParameter("@Offset", offset),
                new SqlParameter("@Sort", sort),
                new SqlParameter("@Order", order),
                };

            List<JobCrudDto> rval = null;
            rval = await _dbContext.JobCruds.FromSql<JobCrudDto>("[System_Get_JobCrudBySubscriber] @SubscriberGuid, @Limit, @Offset, @Sort, @Order", spParams).ToListAsync();
            return rval;
        }

        public async Task<List<UpDiddyLib.Domain.Models.TopicDto>> GetTopics(int limit, int offset, string sort, string order)
        {
            var spParams = new object[] {
                new SqlParameter("@Limit", limit),
                new SqlParameter("@Offset", offset),
                new SqlParameter("@Sort", sort),
                new SqlParameter("@Order", order),
                };

            List<UpDiddyLib.Domain.Models.TopicDto> rval = null;
            rval = await _dbContext.Topics.FromSql<UpDiddyLib.Domain.Models.TopicDto>("System_Get_Topics @Limit, @Offset, @Sort, @Order", spParams).ToListAsync();
            return rval;
        }

        public async Task<List<UpDiddyLib.Domain.Models.CompensationTypeDto>> GetCompensationTypes(int limit, int offset, string sort, string order)
        {
            var spParams = new object[] {
                new SqlParameter("@Limit", limit),
                new SqlParameter("@Offset", offset),
                new SqlParameter("@Sort", sort),
                new SqlParameter("@Order", order),
                };

            List<UpDiddyLib.Domain.Models.CompensationTypeDto> rval = null;
            rval = await _dbContext.CompensationTypes.FromSql<UpDiddyLib.Domain.Models.CompensationTypeDto>("System_Get_CompensationTypes @Limit, @Offset, @Sort, @Order", spParams).ToListAsync();
            return rval;
        }

        public async Task<List<CountryDetailDto>> GetCountries(int limit, int offset, string sort, string order)
        {
            var spParams = new object[] {
                new SqlParameter("@Limit", limit),
                new SqlParameter("@Offset", offset),
                new SqlParameter("@Sort", sort),
                new SqlParameter("@Order", order),
                };

            List<CountryDetailDto> rval = null;
            rval = await _dbContext.Countries.FromSql<CountryDetailDto>("System_Get_Countries @Limit, @Offset, @Sort, @Order", spParams).ToListAsync();
            return rval;
        }

        public async Task<List<CourseLevelDto>> GetCourseLevels(int limit, int offset, string sort, string order)
        {
            var spParams = new object[] {
                new SqlParameter("@Limit", limit),
                new SqlParameter("@Offset", offset),
                new SqlParameter("@Sort", sort),
                new SqlParameter("@Order", order),
                };

            List<CourseLevelDto> rval = null;
            rval = await _dbContext.CourseLevels.FromSql<CourseLevelDto>("System_Get_CourseLevels @Limit, @Offset, @Sort, @Order", spParams).ToListAsync();
            return rval;
        }

        public async Task<List<UpDiddyLib.Domain.Models.EducationLevelDto>> GetEducationLevels(int limit, int offset, string sort, string order)
        {
            var spParams = new object[] {
                new SqlParameter("@Limit", limit),
                new SqlParameter("@Offset", offset),
                new SqlParameter("@Sort", sort),
                new SqlParameter("@Order", order),
                };

            List<UpDiddyLib.Domain.Models.EducationLevelDto> rval = null;
            rval = await _dbContext.EducationLevels.FromSql<UpDiddyLib.Domain.Models.EducationLevelDto>("System_Get_EducationLevels @Limit, @Offset, @Sort, @Order", spParams).ToListAsync();
            return rval;
        }

        public async Task<List<UpDiddyLib.Domain.Models.EducationalDegreeTypeDto>> GetEducationalDegreeTypes(int limit, int offset, string sort, string order)
        {
            var spParams = new object[] {
                new SqlParameter("@Limit", limit),
                new SqlParameter("@Offset", offset),
                new SqlParameter("@Sort", sort),
                new SqlParameter("@Order", order),
                };

            List<UpDiddyLib.Domain.Models.EducationalDegreeTypeDto> rval = null;
            rval = await _dbContext.EducationalDegreeTypes.FromSql<UpDiddyLib.Domain.Models.EducationalDegreeTypeDto>("System_Get_EducationalDegreeTypes @Limit, @Offset, @Sort, @Order", spParams).ToListAsync();
            return rval;
        }

        public async Task<List<UpDiddyLib.Domain.Models.EmploymentTypeDto>> GetEmploymentTypes(int limit, int offset, string sort, string order)
        {
            var spParams = new object[] {
                new SqlParameter("@Limit", limit),
                new SqlParameter("@Offset", offset),
                new SqlParameter("@Sort", sort),
                new SqlParameter("@Order", order),
                };

            List<UpDiddyLib.Domain.Models.EmploymentTypeDto> rval = null;
            rval = await _dbContext.EmploymentTypes.FromSql<UpDiddyLib.Domain.Models.EmploymentTypeDto>("System_Get_EmploymentTypes @Limit, @Offset, @Sort, @Order", spParams).ToListAsync();
            return rval;
        }

        public async Task<List<UpDiddyLib.Domain.Models.ExperienceLevelDto>> GetExperienceLevels(int limit, int offset, string sort, string order)
        {
            var spParams = new object[] {
                new SqlParameter("@Limit", limit),
                new SqlParameter("@Offset", offset),
                new SqlParameter("@Sort", sort),
                new SqlParameter("@Order", order),
                };

            List<UpDiddyLib.Domain.Models.ExperienceLevelDto> rval = null;
            rval = await _dbContext.ExperienceLevels.FromSql<UpDiddyLib.Domain.Models.ExperienceLevelDto>("System_Get_ExperienceLevels @Limit, @Offset, @Sort, @Order", spParams).ToListAsync();
            return rval;
        }

        public async Task<List<UpDiddyLib.Domain.Models.IndustryDto>> GetIndustries(int limit, int offset, string sort, string order)
        {
            var spParams = new object[] {
                new SqlParameter("@Limit", limit),
                new SqlParameter("@Offset", offset),
                new SqlParameter("@Sort", sort),
                new SqlParameter("@Order", order),
                };

            List<UpDiddyLib.Domain.Models.IndustryDto> rval = null;
            rval = await _dbContext.Industries.FromSql<UpDiddyLib.Domain.Models.IndustryDto>("System_Get_Industries @Limit, @Offset, @Sort, @Order", spParams).ToListAsync();
            return rval;
        }

        public async Task<List<UpDiddyLib.Domain.Models.OfferDto>> GetOffers(int limit, int offset, string sort, string order)
        {
            var spParams = new object[] {
                new SqlParameter("@Limit", limit),
                new SqlParameter("@Offset", offset),
                new SqlParameter("@Sort", sort),
                new SqlParameter("@Order", order),
                };

            List<UpDiddyLib.Domain.Models.OfferDto> rval = null;
            rval = await _dbContext.Offers.FromSql<UpDiddyLib.Domain.Models.OfferDto>("System_Get_Offers @Limit, @Offset, @Sort, @Order", spParams).ToListAsync();
            return rval;
        }

        public async Task<List<UpDiddyLib.Domain.Models.SecurityClearanceDto>> GetSecurityClearances(int limit, int offset, string sort, string order)
        {
            var spParams = new object[] {
                new SqlParameter("@Limit", limit),
                new SqlParameter("@Offset", offset),
                new SqlParameter("@Sort", sort),
                new SqlParameter("@Order", order),
                };

            List<UpDiddyLib.Domain.Models.SecurityClearanceDto> rval = null;
            rval = await _dbContext.SecurityClearances.FromSql<UpDiddyLib.Domain.Models.SecurityClearanceDto>("System_Get_SecurityClearances @Limit, @Offset, @Sort, @Order", spParams).ToListAsync();
            return rval;
        }

        public async Task<List<UpDiddyLib.Domain.Models.SkillDto>> GetSkills(int limit, int offset, string sort, string order)
        {
            var spParams = new object[] {
                new SqlParameter("@Limit", limit),
                new SqlParameter("@Offset", offset),
                new SqlParameter("@Sort", sort),
                new SqlParameter("@Order", order),
                };

            List<UpDiddyLib.Domain.Models.SkillDto> rval = null;
            rval = await _dbContext.Skills.FromSql<UpDiddyLib.Domain.Models.SkillDto>("System_Get_Skills @Limit, @Offset, @Sort, @Order", spParams).ToListAsync();
            return rval;
        }

        public async Task<List<UpDiddyLib.Domain.Models.StateDetailDto>> GetStates(Guid country, int limit, int offset, string sort, string order)
        {
            var spParams = new object[] {
                new SqlParameter("@Country", country),
                new SqlParameter("@Limit", limit),
                new SqlParameter("@Offset", offset),
                new SqlParameter("@Sort", sort),
                new SqlParameter("@Order", order),
                };

            List<UpDiddyLib.Domain.Models.StateDetailDto> rval = null;
            rval = await _dbContext.States.FromSql<UpDiddyLib.Domain.Models.StateDetailDto>("System_Get_States @Country, @Limit, @Offset, @Sort, @Order", spParams).ToListAsync();
            return rval;
        }

        public async Task<List<UpDiddyLib.Domain.Models.TalentFavoriteDto>> GetTalentFavorites(Guid subscriber, int limit, int offset, string sort, string order)
        {
            var spParams = new object[] {
                new SqlParameter("@Subscriber", subscriber),
                new SqlParameter("@Limit", limit),
                new SqlParameter("@Offset", offset),
                new SqlParameter("@Sort", sort),
                new SqlParameter("@Order", order),
                };

            List<UpDiddyLib.Domain.Models.TalentFavoriteDto> rval = null;
            rval = await _dbContext.TalentFavorites.FromSql<UpDiddyLib.Domain.Models.TalentFavoriteDto>("System_Get_TalentFavorites @Subscriber, @Limit, @Offset, @Sort, @Order", spParams).ToListAsync();
            return rval;
        }

        public async Task<List<SubscriberNotesDto>> GetSubscriberNotes(Guid subscriber, int limit, int offset, string sort, string order)
        {
            var spParams = new object[] {
                new SqlParameter("@Subscriber", subscriber),
                new SqlParameter("@Limit", limit),
                new SqlParameter("@Offset", offset),
                new SqlParameter("@Sort", sort),
                new SqlParameter("@Order", order),
                };

            List<SubscriberNotesDto> rval = null;
            rval = await _dbContext.SubscriberNotesDto.FromSql<SubscriberNotesDto>("System_Get_SubscriberNotes @Subscriber, @Limit, @Offset, @Sort, @Order", spParams).ToListAsync();
            return rval;
        }

        public async Task<List<UpDiddyLib.Domain.Models.PartnerDto>> GetPartners(int limit, int offset, string sort, string order)
        {
            var spParams = new object[] {
                new SqlParameter("@Limit", limit),
                new SqlParameter("@Offset", offset),
                new SqlParameter("@Sort", sort),
                new SqlParameter("@Order", order),
                };

            List<UpDiddyLib.Domain.Models.PartnerDto> rval = null;
            rval = await _dbContext.Partners.FromSql<UpDiddyLib.Domain.Models.PartnerDto>("System_Get_Partners @Limit, @Offset, @Sort, @Order", spParams).ToListAsync();
            return rval;
        }

        public async Task<int> UpdateNotificationCoursesAsync(Guid subscriberGuid, Guid notificationGuid, List<Guid> groups)
        {

            DataTable groupTable = new DataTable();
            groupTable.Columns.Add("Guid", typeof(Guid));
            if (groups != null && groups.Count > 0)
            {
                foreach (var groupGuid in groups)
                {
                    groupTable.Rows.Add(groupGuid);
                }
            }


            var subscriberParam = new SqlParameter("@SubscriberGuid", SqlDbType.UniqueIdentifier);
            subscriberParam.Value = subscriberGuid;

            var notificationParam = new SqlParameter("@NotificationGuid", SqlDbType.UniqueIdentifier);
            notificationParam.Value = notificationGuid;

            var groupParams = new SqlParameter("@GroupGuids", groupTable);
            groupParams.SqlDbType = SqlDbType.Structured;
            groupParams.TypeName = "dbo.GuidList";

            var spParams = new object[] { subscriberParam, notificationParam, groupParams };



            var rowsAffected = _dbContext.Database.ExecuteSqlCommand(@"
                EXEC [dbo].[System_Update_NotificationGroups] 
                    @SubscriberGuid,
                    @NotificationGuid,
	                @GroupGuids", spParams);

            return rowsAffected;

        }

        public async Task<List<GroupInfoDto>> GetGroups(int limit, int offset, string sort, string order)
        {
            var spParams = new object[] {
                new SqlParameter("@Limit", limit),
                new SqlParameter("@Offset", offset),
                new SqlParameter("@Sort", sort),
                new SqlParameter("@Order", order),
                };

            List<GroupInfoDto> rval = null;
            rval = await _dbContext.Groups.FromSql<GroupInfoDto>("[System_Get_Groups]  @Limit, @Offset, @Sort, @Order", spParams).ToListAsync();
            return rval;
        }

        public async Task UpdateEntitySkills(Guid entityGuid, string entityType, List<Guid> skillGuids)
        {
            DataTable skillTable = new DataTable();
            skillTable.Columns.Add("Guid", typeof(Guid));
            if (skillGuids != null && skillGuids.Count > 0)
            {
                foreach (var skillGuid in skillGuids)
                {
                    skillTable.Rows.Add(skillGuid);
                }
            }

            var entityParam = new SqlParameter("@EntityGuid", SqlDbType.UniqueIdentifier);
            entityParam.Value = entityGuid;

            var entityTypeParam = new SqlParameter("@EntityType", SqlDbType.VarChar, 100);
            entityTypeParam.Value = entityType;

            var skillsParam = new SqlParameter("@SkillGuids", skillTable);
            skillsParam.SqlDbType = SqlDbType.Structured;
            skillsParam.TypeName = "dbo.GuidList";

            var spParams = new object[] { entityParam, entityTypeParam, skillsParam };

            var rowsAffected = _dbContext.Database.ExecuteSqlCommand(@"
                EXEC [dbo].[System_Update_EntitySkills] 
                    @EntityGuid,
                    @EntityType,
	                @SkillGuids", spParams);
        }

        public async Task<List<RecruiterInfoDto>> GetRecruiters(int limit, int offset, string sort, string order)
        {
            var spParams = new object[] {
                new SqlParameter("@Limit", limit),
                new SqlParameter("@Offset", offset),
                new SqlParameter("@Sort", sort),
                new SqlParameter("@Order", order),
                };

            List<RecruiterInfoDto> rval = null;
            rval = await _dbContext.Recruiters.FromSql<RecruiterInfoDto>("[System_Get_Recruiters]  @Limit, @Offset, @Sort, @Order", spParams).ToListAsync();
            return rval;
        }

        public async Task<List<UpDiddyLib.Domain.Models.JobSiteScrapeStatisticDto>> GetJobSiteScrapeStatistics(int limit, int offset, string sort, string order)
        {
            var spParams = new object[] {
                new SqlParameter("@Limit", limit),
                new SqlParameter("@Offset", offset),
                new SqlParameter("@Sort", sort),
                new SqlParameter("@Order", order),
                };

            List<UpDiddyLib.Domain.Models.JobSiteScrapeStatisticDto> rval = null;
            rval = await _dbContext.JobSiteScrapeStatistics.FromSql<UpDiddyLib.Domain.Models.JobSiteScrapeStatisticDto>("System_Get_JobSiteScrapeStatistics @Limit, @Offset, @Sort, @Order", spParams).ToListAsync();
            return rval;
        }

        public async Task<List<UsersDto>> GetNewUsers()
        {
            List<UsersDto> rval = null;
            rval = await _dbContext.Users.FromSql<UsersDto>("[System_Report_NewUsers]").ToListAsync();
            return rval;
        }

        public async Task<List<UsersDetailDto>> GetAllUsersDetail()
        {
            List<UsersDetailDto> rval = null;
            rval = await _dbContext.UsersDetail.FromSql<UsersDetailDto>("[System_Report_AllUsersDetail]").ToListAsync();
            return rval;
        }

        public async Task<List<UsersDetailDto>> GetUsersByPartnerDetail(Guid partner, DateTime startDate, DateTime endDate)
        {
            var spParams = new object[] {
                new SqlParameter("@Partner", partner),
                new SqlParameter("@StartDate", startDate),
                new SqlParameter("@EndDate", endDate)
                };

            List<UsersDetailDto> rval = null;
            rval = await _dbContext.UsersDetail.FromSql<UsersDetailDto>("[System_Report_UsersByPartnerDetail] @Partner, @StartDate, @EndDate", spParams).ToListAsync();
            return rval;
        }

        public async Task<List<PartnerUsers>> GetUsersByPartner(DateTime startDate, DateTime endDate)
        {
            var spParams = new object[] {
                new SqlParameter("@StartDate", startDate),
                new SqlParameter("@EndDate", endDate)
                };

            List<PartnerUsers> rval = null;
            rval = await _dbContext.PartnerUsers.FromSql<PartnerUsers>("[System_Report_UsersByPartner] @StartDate, @EndDate", spParams).ToListAsync();
            return rval;
        }

        public async Task<bool> InsertSendGridEvents(string sendGridJson)
        {
            var spParams = new object[] {
                new SqlParameter("@SendGridJson", sendGridJson),

                };

            List<JobCrudDto> rval = null;
            _dbContext.Database.ExecuteSqlCommand("[System_Insert_SendGridEvents]  @SendGridJson", spParams);
            return true;
        }

        public async Task<bool> PurgeSendGridEvents(int LookbackDays)
        {
            var spParams = new object[] {
                new SqlParameter("@LookbackDays", LookbackDays),

                };

            _dbContext.Database.ExecuteSqlCommand("[System_Purge_SendGridEvents]  @LookbackDays", spParams);
            return true;
        }

        public async Task<List<SubscriberEmailStatisticDto>> GetSubscriberEmailStatistics(string email)
        {
            var spParams = new object[] {
                new SqlParameter("@EmailAddress", email)
             };

            List<SubscriberEmailStatisticDto> rval = null;
            rval = await _dbContext.SubscriberEmailStatistics.FromSql<SubscriberEmailStatisticDto>("[System_Get_SubscriberEmailStatistics] @EmailAddress", spParams).ToListAsync();
            return rval;

        }

        public async Task<Tuple<Guid?, string>> CreateJobPosting(JobCrudDto jobCrudDto)
        {
            var postingDateUTC = new SqlParameter { ParameterName = "@PostingDateUTC", SqlDbType = SqlDbType.DateTime, Direction = ParameterDirection.Input, Value = jobCrudDto.PostingDateUTC };
            var postingExpirationDateUTC = new SqlParameter { ParameterName = "@PostingExpirationDateUTC", SqlDbType = SqlDbType.DateTime, Direction = ParameterDirection.Input, Value = jobCrudDto.PostingExpirationDateUTC };
            var applicationDeadlineUTC = new SqlParameter { ParameterName = "@ApplicationDeadlineUTC", SqlDbType = SqlDbType.DateTime, Direction = ParameterDirection.Input, Value = jobCrudDto.ApplicationDeadlineUTC };
            var title = new SqlParameter { ParameterName = "@Title", SqlDbType = SqlDbType.NVarChar, Direction = ParameterDirection.Input, Value = (object)jobCrudDto.Title ?? DBNull.Value };
            var description = new SqlParameter { ParameterName = "@Description", SqlDbType = SqlDbType.NVarChar, Direction = ParameterDirection.Input, Value = (object)jobCrudDto.Description ?? DBNull.Value };
            var h2Visa = new SqlParameter { ParameterName = "@H2Visa", SqlDbType = SqlDbType.Bit, Direction = ParameterDirection.Input, Value = (object)jobCrudDto.H2Visa ?? false };
            var isAgencyJobPosting = new SqlParameter { ParameterName = "@IsAgencyJobPosting", SqlDbType = SqlDbType.Bit, Direction = ParameterDirection.Input, Value = (object)jobCrudDto.IsAgencyJobPosting ?? false };
            var telecommutePercentage = new SqlParameter { ParameterName = "@TelecommutePercentage", SqlDbType = SqlDbType.Int, Direction = ParameterDirection.Input, Value = (object)jobCrudDto.TelecommutePercentage ?? 0 };
            var compensation = new SqlParameter { ParameterName = "@Compensation", SqlDbType = SqlDbType.Decimal, Direction = ParameterDirection.Input, Value = (object)jobCrudDto.Compensation ?? 0 };
            var thirdPartyApply = new SqlParameter { ParameterName = "@ThirdPartyApply", SqlDbType = SqlDbType.Bit, Direction = ParameterDirection.Input, Value = (object)jobCrudDto.ThirdPartyApply ?? false };
            var thirdPartyApplicationUrl = new SqlParameter { ParameterName = "@ThirdPartyApplicationUrl", SqlDbType = SqlDbType.NVarChar, Direction = ParameterDirection.Input, Value = (object)jobCrudDto.ThirdPartyApplicationUrl ?? DBNull.Value };
            var country = new SqlParameter { ParameterName = "@Country", SqlDbType = SqlDbType.NVarChar, Direction = ParameterDirection.Input, Value = (object)jobCrudDto.Country ?? DBNull.Value };
            var city = new SqlParameter { ParameterName = "@City", SqlDbType = SqlDbType.NVarChar, Direction = ParameterDirection.Input, Value = (object)jobCrudDto.City ?? DBNull.Value };
            var province = new SqlParameter { ParameterName = "@Province", SqlDbType = SqlDbType.NVarChar, Direction = ParameterDirection.Input, Value = (object)jobCrudDto.Province ?? DBNull.Value };
            var postalCode = new SqlParameter { ParameterName = "@PostalCode", SqlDbType = SqlDbType.NVarChar, Direction = ParameterDirection.Input, Value = (object)jobCrudDto.PostalCode ?? DBNull.Value };
            var streetAddress = new SqlParameter { ParameterName = "@StreetAddress", SqlDbType = SqlDbType.NVarChar, Direction = ParameterDirection.Input, Value = (object)jobCrudDto.StreetAddress ?? DBNull.Value };
            var isPrivate = new SqlParameter { ParameterName = "@IsPrivate", SqlDbType = SqlDbType.Int, Direction = ParameterDirection.Input, Value = (object)jobCrudDto.IsPrivate ?? 0 };
            var jobStatus = new SqlParameter { ParameterName = "@JobStatus", SqlDbType = SqlDbType.Int, Direction = ParameterDirection.Input, Value = (object)jobCrudDto.JobStatus ?? 0 };
            var thirdPartyIdentifier = new SqlParameter { ParameterName = "@ThirdPartyIdentifier", SqlDbType = SqlDbType.NVarChar, Direction = ParameterDirection.Input, Value = (object)jobCrudDto.ThirdPartyIdentifier ?? DBNull.Value };
            var recruiterGuid = new SqlParameter { ParameterName = "@RecruiterGuid", SqlDbType = SqlDbType.UniqueIdentifier, Direction = ParameterDirection.Input, Value = jobCrudDto.RecruiterGuid };
            var companyGuid = new SqlParameter { ParameterName = "@CompanyGuid", SqlDbType = SqlDbType.UniqueIdentifier, Direction = ParameterDirection.Input, Value = jobCrudDto.CompanyGuid };
            var industryGuid = new SqlParameter { ParameterName = "@IndustryGuid", SqlDbType = SqlDbType.UniqueIdentifier, Direction = ParameterDirection.Input, Value = jobCrudDto.IndustryGuid };
            var jobCategoryGuid = new SqlParameter { ParameterName = "@JobCategoryGuid", SqlDbType = SqlDbType.UniqueIdentifier, Direction = ParameterDirection.Input, Value = jobCrudDto.JobCategoryGuid };
            var experienceLevelGuid = new SqlParameter { ParameterName = "@ExperienceLevelGuid", SqlDbType = SqlDbType.UniqueIdentifier, Direction = ParameterDirection.Input, Value = jobCrudDto.ExperienceLevelGuid };
            var educationLevelGuid = new SqlParameter { ParameterName = "@EducationLevelGuid", SqlDbType = SqlDbType.UniqueIdentifier, Direction = ParameterDirection.Input, Value = jobCrudDto.EducationLevelGuid };
            var compensationTypeGuid = new SqlParameter { ParameterName = "@CompensationTypeGuid", SqlDbType = SqlDbType.UniqueIdentifier, Direction = ParameterDirection.Input, Value = jobCrudDto.CompensationTypeGuid };
            var securityClearanceGuid = new SqlParameter { ParameterName = "@SecurityClearanceGuid", SqlDbType = SqlDbType.UniqueIdentifier, Direction = ParameterDirection.Input, Value = jobCrudDto.SecurityClearanceGuid };
            var employmentTypeGuid = new SqlParameter { ParameterName = "@EmploymentTypeGuid", SqlDbType = SqlDbType.UniqueIdentifier, Direction = ParameterDirection.Input, Value = jobCrudDto.EmploymentTypeGuid };
            var jobPostingGuid = new SqlParameter { ParameterName = "@JobPostingGuid", SqlDbType = SqlDbType.UniqueIdentifier, Size = -1, Direction = ParameterDirection.Output };
            var validationErrors = new SqlParameter { ParameterName = "@ValidationErrors", SqlDbType = SqlDbType.NVarChar, Size = -1, Direction = ParameterDirection.Output };
            var spParams = new object[] { postingDateUTC, postingExpirationDateUTC, applicationDeadlineUTC, title, description, h2Visa, isAgencyJobPosting, telecommutePercentage, compensation, thirdPartyApply, thirdPartyApplicationUrl, country, city, province, postalCode, streetAddress, isPrivate, jobStatus, thirdPartyIdentifier, recruiterGuid, companyGuid, industryGuid, jobCategoryGuid, experienceLevelGuid, educationLevelGuid, compensationTypeGuid, securityClearanceGuid, employmentTypeGuid, jobPostingGuid, validationErrors };

            var rowsAffected = _dbContext.Database.ExecuteSqlCommand(@"EXEC [dbo].[System_Create_JobPosting] @PostingDateUTC, @PostingExpirationDateUTC, @ApplicationDeadlineUTC, @Title, @Description, @H2Visa, @IsAgencyJobPosting, @TelecommutePercentage, @Compensation, @ThirdPartyApply, @ThirdPartyApplicationUrl, @Country, @City, @Province, @PostalCode, @StreetAddress, @IsPrivate, @JobStatus, @ThirdPartyIdentifier, @RecruiterGuid, @CompanyGuid, @IndustryGuid, @JobCategoryGuid, @ExperienceLevelGuid, @EducationLevelGuid, @CompensationTypeGuid, @SecurityClearanceGuid, @EmploymentTypeGuid, @JobPostingGuid OUTPUT, @ValidationErrors OUTPUT", spParams);
            Guid response;
            if (Guid.TryParse(jobPostingGuid.Value.ToString(), out response))
                return new Tuple<Guid?, string>(response, null);
            else
                return new Tuple<Guid?, string>(null, validationErrors.Value.ToString());
        }

        public async Task<string> UpdateJobPosting(JobCrudDto jobCrudDto)
        {
            var jobPostingGuid = new SqlParameter { ParameterName = "@JobPostingGuid", SqlDbType = SqlDbType.UniqueIdentifier, Direction = ParameterDirection.Input, Value = jobCrudDto.JobPostingGuid };
            var postingDateUTC = new SqlParameter { ParameterName = "@PostingDateUTC", SqlDbType = SqlDbType.DateTime, Direction = ParameterDirection.Input, Value = jobCrudDto.PostingDateUTC };
            var postingExpirationDateUTC = new SqlParameter { ParameterName = "@PostingExpirationDateUTC", SqlDbType = SqlDbType.DateTime, Direction = ParameterDirection.Input, Value = jobCrudDto.PostingExpirationDateUTC };
            var applicationDeadlineUTC = new SqlParameter { ParameterName = "@ApplicationDeadlineUTC", SqlDbType = SqlDbType.DateTime, Direction = ParameterDirection.Input, Value = jobCrudDto.ApplicationDeadlineUTC };
            var title = new SqlParameter { ParameterName = "@Title", SqlDbType = SqlDbType.NVarChar, Direction = ParameterDirection.Input, Value = (object)jobCrudDto.Title ?? DBNull.Value };
            var description = new SqlParameter { ParameterName = "@Description", SqlDbType = SqlDbType.NVarChar, Direction = ParameterDirection.Input, Value = (object)jobCrudDto.Description ?? DBNull.Value };
            var h2Visa = new SqlParameter { ParameterName = "@H2Visa", SqlDbType = SqlDbType.Bit, Direction = ParameterDirection.Input, Value = (object)jobCrudDto.H2Visa ?? false };
            var isAgencyJobPosting = new SqlParameter { ParameterName = "@IsAgencyJobPosting", SqlDbType = SqlDbType.Bit, Direction = ParameterDirection.Input, Value = (object)jobCrudDto.IsAgencyJobPosting ?? false };
            var telecommutePercentage = new SqlParameter { ParameterName = "@TelecommutePercentage", SqlDbType = SqlDbType.Int, Direction = ParameterDirection.Input, Value = (object)jobCrudDto.TelecommutePercentage ?? 0 };
            var compensation = new SqlParameter { ParameterName = "@Compensation", SqlDbType = SqlDbType.Decimal, Direction = ParameterDirection.Input, Value = (object)jobCrudDto.Compensation ?? 0 };
            var thirdPartyApply = new SqlParameter { ParameterName = "@ThirdPartyApply", SqlDbType = SqlDbType.Bit, Direction = ParameterDirection.Input, Value = (object)jobCrudDto.ThirdPartyApply ?? false };
            var thirdPartyApplicationUrl = new SqlParameter { ParameterName = "@ThirdPartyApplicationUrl", SqlDbType = SqlDbType.NVarChar, Direction = ParameterDirection.Input, Value = (object)jobCrudDto.ThirdPartyApplicationUrl ?? DBNull.Value };
            var country = new SqlParameter { ParameterName = "@Country", SqlDbType = SqlDbType.NVarChar, Direction = ParameterDirection.Input, Value = (object)jobCrudDto.Country ?? DBNull.Value };
            var city = new SqlParameter { ParameterName = "@City", SqlDbType = SqlDbType.NVarChar, Direction = ParameterDirection.Input, Value = (object)jobCrudDto.City ?? DBNull.Value };
            var province = new SqlParameter { ParameterName = "@Province", SqlDbType = SqlDbType.NVarChar, Direction = ParameterDirection.Input, Value = (object)jobCrudDto.Province ?? DBNull.Value };
            var postalCode = new SqlParameter { ParameterName = "@PostalCode", SqlDbType = SqlDbType.NVarChar, Direction = ParameterDirection.Input, Value = (object)jobCrudDto.PostalCode ?? DBNull.Value };
            var streetAddress = new SqlParameter { ParameterName = "@StreetAddress", SqlDbType = SqlDbType.NVarChar, Direction = ParameterDirection.Input, Value = (object)jobCrudDto.StreetAddress ?? DBNull.Value };
            var isPrivate = new SqlParameter { ParameterName = "@IsPrivate", SqlDbType = SqlDbType.Int, Direction = ParameterDirection.Input, Value = (object)jobCrudDto.IsPrivate ?? 0 };
            var jobStatus = new SqlParameter { ParameterName = "@JobStatus", SqlDbType = SqlDbType.Int, Direction = ParameterDirection.Input, Value = (object)jobCrudDto.JobStatus ?? 0 };
            var thirdPartyIdentifier = new SqlParameter { ParameterName = "@ThirdPartyIdentifier", SqlDbType = SqlDbType.NVarChar, Direction = ParameterDirection.Input, Value = (object)jobCrudDto.ThirdPartyIdentifier ?? DBNull.Value };
            var recruiterGuid = new SqlParameter { ParameterName = "@RecruiterGuid", SqlDbType = SqlDbType.UniqueIdentifier, Direction = ParameterDirection.Input, Value = jobCrudDto.RecruiterGuid };
            var companyGuid = new SqlParameter { ParameterName = "@CompanyGuid", SqlDbType = SqlDbType.UniqueIdentifier, Direction = ParameterDirection.Input, Value = jobCrudDto.CompanyGuid };
            var industryGuid = new SqlParameter { ParameterName = "@IndustryGuid", SqlDbType = SqlDbType.UniqueIdentifier, Direction = ParameterDirection.Input, Value = jobCrudDto.IndustryGuid };
            var jobCategoryGuid = new SqlParameter { ParameterName = "@JobCategoryGuid", SqlDbType = SqlDbType.UniqueIdentifier, Direction = ParameterDirection.Input, Value = jobCrudDto.JobCategoryGuid };
            var experienceLevelGuid = new SqlParameter { ParameterName = "@ExperienceLevelGuid", SqlDbType = SqlDbType.UniqueIdentifier, Direction = ParameterDirection.Input, Value = jobCrudDto.ExperienceLevelGuid };
            var educationLevelGuid = new SqlParameter { ParameterName = "@EducationLevelGuid", SqlDbType = SqlDbType.UniqueIdentifier, Direction = ParameterDirection.Input, Value = jobCrudDto.EducationLevelGuid };
            var compensationTypeGuid = new SqlParameter { ParameterName = "@CompensationTypeGuid", SqlDbType = SqlDbType.UniqueIdentifier, Direction = ParameterDirection.Input, Value = jobCrudDto.CompensationTypeGuid };
            var securityClearanceGuid = new SqlParameter { ParameterName = "@SecurityClearanceGuid", SqlDbType = SqlDbType.UniqueIdentifier, Direction = ParameterDirection.Input, Value = jobCrudDto.SecurityClearanceGuid };
            var employmentTypeGuid = new SqlParameter { ParameterName = "@EmploymentTypeGuid", SqlDbType = SqlDbType.UniqueIdentifier, Direction = ParameterDirection.Input, Value = jobCrudDto.EmploymentTypeGuid };
            var validationErrors = new SqlParameter { ParameterName = "@ValidationErrors", SqlDbType = SqlDbType.NVarChar, Size = -1, Direction = ParameterDirection.Output };
            var spParams = new object[] { jobPostingGuid, postingDateUTC, postingExpirationDateUTC, applicationDeadlineUTC, title, description, h2Visa, isAgencyJobPosting, telecommutePercentage, compensation, thirdPartyApply, thirdPartyApplicationUrl, country, city, province, postalCode, streetAddress, isPrivate, jobStatus, thirdPartyIdentifier, recruiterGuid, companyGuid, industryGuid, jobCategoryGuid, experienceLevelGuid, educationLevelGuid, compensationTypeGuid, securityClearanceGuid, employmentTypeGuid, validationErrors };

            var rowsAffected = _dbContext.Database.ExecuteSqlCommand(@"EXEC [dbo].[System_Update_JobPosting] @JobPostingGuid, @PostingDateUTC, @PostingExpirationDateUTC, @ApplicationDeadlineUTC, @Title, @Description, @H2Visa, @IsAgencyJobPosting, @TelecommutePercentage, @Compensation, @ThirdPartyApply, @ThirdPartyApplicationUrl, @Country, @City, @Province, @PostalCode, @StreetAddress, @IsPrivate, @JobStatus, @ThirdPartyIdentifier, @RecruiterGuid, @CompanyGuid, @IndustryGuid, @JobCategoryGuid, @ExperienceLevelGuid, @EducationLevelGuid, @CompensationTypeGuid, @SecurityClearanceGuid, @EmploymentTypeGuid, @ValidationErrors OUTPUT", spParams);
            return validationErrors.Value.ToString();
        }

        public async Task UpdateJobPostingSkills( Guid jobPostingGuid, List<Guid> skillGuids)
        {
            var jobPostingGuidParam = new SqlParameter { ParameterName = "@JobPostingGuid", SqlDbType = SqlDbType.UniqueIdentifier, Direction = ParameterDirection.Input, Value = jobPostingGuid };

            DataTable table = new DataTable();
            table.Columns.Add("Guid", typeof(Guid));
            if (skillGuids != null)
            {
                foreach (var skillGuid in skillGuids)
                {
                    table.Rows.Add(skillGuid);
                }
            }

            var skillGuidsParam = new SqlParameter("@SkillGuids", table);
            skillGuidsParam.SqlDbType = SqlDbType.Structured;
            skillGuidsParam.TypeName = "dbo.GuidList";

            var spParams = new object[] { jobPostingGuidParam, skillGuidsParam };

            var rowsAffected = _dbContext.Database.ExecuteSqlCommand(@"EXEC [dbo].[System_Update_JobPostingSkillsByGuid] @JobPostingGuid, @SkillGuids", spParams);
        }


        /// <summary>
        /// Invoke stored procedure to create g2 records for the specified subscriber.  1 record per active company will be created.  If an existing record for the
        /// subscriber/company combination already exists, it will NOT be duplicated.
        /// </summary>
        /// <param name="subscriberGuid"></param>
        /// <returns></returns>
        public async Task<int> CreateSubscriberG2Profiles(Guid subscriberGuid)
        {
            var subscriber = new SqlParameter("@SubscriberGuid", SqlDbType.UniqueIdentifier);
            subscriber.Value = subscriberGuid; 
            subscriber.Direction = ParameterDirection.Input;
        
            var spParams = new object[] { subscriber };
            var rowsAffected = _dbContext.Database.ExecuteSqlCommand(@"EXEC [dbo].[System_Create_SubscriberG2Profiles] @SubscriberGuid", spParams);
            return rowsAffected;          
            
        }

        /// <summary>
        /// Invoke stored procedure to delete g2 records for the specified subscriber  
        /// </summary>
        /// <param name="subscriberGuid"></param>
        /// <returns></returns>
        public async Task<int> DeleteSubscriberG2Profiles(Guid subscriberGuid)
        {
            var subscriber = new SqlParameter("@SubscriberGuid", SqlDbType.UniqueIdentifier);
            subscriber.Value = subscriberGuid;
            subscriber.Direction = ParameterDirection.Input;

            var spParams = new object[] { subscriber };
            var rowsAffected = _dbContext.Database.ExecuteSqlCommand(@"EXEC [dbo].[System_Delete_SubscriberG2Profiles] @SubscriberGuid", spParams);
            return rowsAffected;

        }
 
        /// <summary>
        /// Invoke stored procedure to create g2 records for the specified company.  1 record per active subscriber will be created.  If an existing record for the
        /// subscriber/company combination already exists, it will NOT be duplicated.
        /// </summary>
        /// <param name="subscriberGuid"></param>
        /// <returns></returns>
        public async Task<int> CreateCompanyG2Profiles(Guid companyGuid)
        {
            var subscriber = new SqlParameter("@CompanyGuid", SqlDbType.UniqueIdentifier);
            subscriber.Value = companyGuid;
            subscriber.Direction = ParameterDirection.Input;

            var spParams = new object[] { subscriber };
            var rowsAffected = _dbContext.Database.ExecuteSqlCommand(@"EXEC [dbo].[System_Create_CompanyG2Profiles] @CompanyGuid", spParams);
            return rowsAffected;

        }

        /// <summary>
        /// Invoke stored procedure to delete g2 records for the specified company.
        /// </summary>
        /// <param name="companyGuid"></param>
        /// <returns></returns>
        public async Task<int> DeleteCompanyG2Profiles(Guid companyGuid)
        {
            var company = new SqlParameter("@CompanyGuid", SqlDbType.UniqueIdentifier);
            company.Value = companyGuid;
            company.Direction = ParameterDirection.Input;

            var spParams = new object[] { company };
            var rowsAffected = _dbContext.Database.ExecuteSqlCommand(@"EXEC [dbo].[System_Delete_CompanyG2Profiles] @CompanyGuid", spParams);
            return rowsAffected;

        }

        // todo jab add migration for updated SPROC 
        public async Task<bool> UpdateG2AzureIndexStatuses(List<AzureIndexResultStatus> indexStatuses, string statusName, string statusInfo)
        {
 
            DataTable table = new DataTable();
            table.Columns.Add("ErrorMessage", typeof(string));
            table.Columns.Add("ProfileGuid", typeof(Guid));
            table.Columns.Add("IndexStatus", typeof(int));
            if (indexStatuses != null)
            {
                foreach (AzureIndexResultStatus Status in indexStatuses)
                {
                    Guid ProfileGuid = Guid.Parse(Status.Key);

                    DataRow row = table.Rows.Add();
                    row["IndexStatus"] = Status.StatusCode;
                    row["ErrorMessage"] = Status.ErrorMessage;
                    row["ProfileGuid"] = ProfileGuid;

                }
            }

            var profileGuidsParam = new SqlParameter("@ProfileIndexStatuses", table);
            profileGuidsParam.SqlDbType = SqlDbType.Structured;
            profileGuidsParam.TypeName = "dbo.AzureIndexStatus";

            var spParams = new object[] {
                profileGuidsParam
                ,new SqlParameter("@StatusName", statusName)
                ,new SqlParameter("@IndexStatusInfo", statusInfo)
            };        
            var rowsAffected = _dbContext.Database.ExecuteSqlCommand(@"EXEC [g2].[System_Update_AzureG2Status] @ProfileIndexStatuses, @StatusName, @IndexStatusInfo", spParams);
    
            return true;
        }


        public async Task<int>  BootG2Profiles()
        {
            var rowsAffected = _dbContext.Database.ExecuteSqlCommand(@"EXEC [dbo].[System_Create_G2Profiles]");
            return rowsAffected;
        }


    }
}