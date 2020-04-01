using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using UpDiddyLib.Domain.Models;
using UpDiddyApi.ApplicationCore.Exceptions;
using UpDiddyApi.Models.G2;
using System.Data.SqlClient;
using System.Data;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class SkillRepository : UpDiddyRepositoryBase<Skill>, ISkillRepository
    {
        private UpDiddyDbContext _dbContext;
        public SkillRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public IQueryable<Skill> GetAllSkillsQueryable()
        {
            return GetAll();
        }

        public async Task<List<Skill>> GetAllSkillsForJobPostings()
        {
            var skills = await (from skill in _dbContext.Skill
                                join jobPostingSkill in _dbContext.JobPostingSkill on skill.SkillId equals jobPostingSkill.SkillId
                                where skill.IsDeleted.Equals(0) && jobPostingSkill.IsDeleted.Equals(0)
                                select skill).Distinct().ToListAsync();

            return skills;
        }

        public async Task<List<Skill>> GetBySubscriberGuid(Guid subscriberGuid)
        {
            var skills = await (from ss in _dbContext.SubscriberSkill
                                join s in _dbContext.Skill on ss.SkillId equals s.SkillId
                                join su in _dbContext.Subscriber on ss.SubscriberId equals su.SubscriberId
                                where su.SubscriberGuid == subscriberGuid && ss.IsDeleted == 0
                                select s).ToListAsync();

            return skills;
        }

        public async Task<Skill> GetByName(string name)
        {
            return await (from s in _dbContext.Skill
                          where s.SkillName == name
                          select s).FirstOrDefaultAsync();
        }

        public async Task<List<Skill>> GetByTopicGuid(Guid topicGuid)
        {
            return await (from tc in _dbContext.TagCourse
                          join ta in _dbContext.Tag on tc.TagId equals ta.TagId
                          join tt in _dbContext.TagTopic on ta.TagId equals tt.TagId
                          join topic in _dbContext.Topic on tt.TopicId equals topic.TopicId
                          join c in _dbContext.Course on tc.CourseId equals c.CourseId
                          join cs in _dbContext.CourseSkill on c.CourseId equals cs.CourseId
                          join s in _dbContext.Skill on cs.SkillId equals s.SkillId
                          where topic.TopicGuid == topicGuid && topic.IsDeleted == 0 && c.IsDeleted == 0 && cs.IsDeleted == 0
                          select s).Distinct().ToListAsync();
        }

        public async Task<List<Skill>> GetByCourseGuid(Guid courseGuid)
        {
            var skills = await (from ss in _dbContext.CourseSkill
                                join s in _dbContext.Skill on ss.SkillId equals s.SkillId
                                join c in _dbContext.Course on ss.CourseId equals c.CourseId
                                where c.CourseGuid == courseGuid && ss.IsDeleted == 0
                                select s).ToListAsync();

            return skills;
        }

        public async Task<List<SkillDto>> GetProfileSkillsForRecruiter(Guid profileGuid, Guid subscriberGuid, int limit, int offset, string sort, string order)
        {
            var spParams = new object[] {
                new SqlParameter("@ProfileGuid", profileGuid),
                new SqlParameter("@SubscriberGuid", subscriberGuid),
                new SqlParameter("@Limit", limit),
                new SqlParameter("@Offset", offset),
                new SqlParameter("@Sort", sort),
                new SqlParameter("@Order", order),
                };
            List<SkillDto> profileSkills = null;
            profileSkills = await _dbContext.Skills.FromSql<SkillDto>("[G2].[System_Get_ProfileSkillsForRecruiter] @ProfileGuid, @SubscriberGuid, @Limit, @Offset, @Sort, @Order", spParams).ToListAsync();
            return profileSkills;
        }

        public async Task<List<Guid>> AddSkillsToProfileForRecruiter(Guid subscriberGuid, List<Guid> skillGuids, Guid profileGuid)
        {
            bool isProfileAssociatedWithRecruiterCompany = (from p in _dbContext.Profile
                                                            join c in _dbContext.Company on p.CompanyId equals c.CompanyId
                                                            join rc in _dbContext.RecruiterCompany on c.CompanyId equals rc.CompanyId
                                                            join r in _dbContext.Recruiter on rc.RecruiterId equals r.RecruiterId
                                                            join s in _dbContext.Subscriber on r.SubscriberId equals s.SubscriberId
                                                            where s.SubscriberGuid == subscriberGuid && p.ProfileGuid == profileGuid
                                                            select rc.RecruiterCompanyId).Any();
            if (!isProfileAssociatedWithRecruiterCompany)
                throw new FailedValidationException($"recruiter does not have permission to modify profile skills");

            List<Guid> profileSkillGuids = new List<Guid>();
            foreach (Guid skillGuid in skillGuids)
            {
                Guid profileSkillGuid = Guid.NewGuid();
                profileSkillGuids.Add(profileSkillGuid);
                bool isSkillAlreadyAddedProfile = (from ps in _dbContext.ProfileSkill
                                                   join p in _dbContext.Profile on ps.ProfileId equals p.ProfileId
                                                   join s in _dbContext.Skill on ps.SkillId equals s.SkillId
                                                   where s.SkillGuid == skillGuid && p.ProfileGuid == profileGuid
                                                   select ps.ProfileSkillId).Any();
                if (isSkillAlreadyAddedProfile)
                    throw new FailedValidationException($"The skill '{skillGuid}' is already associated with this profile");

                var profileId = (from p in _dbContext.Profile
                                 where p.ProfileGuid == profileGuid && p.IsDeleted == 0
                                 select p.ProfileId).FirstOrDefault();
                if (profileId == null || profileId == 0)
                    throw new FailedValidationException("profile not found");

                var skillId = (from s in _dbContext.Skill
                               where s.SkillGuid == skillGuid && s.IsDeleted == 0
                               select s.SkillId).FirstOrDefault();
                if (skillId == null || skillId == 0)
                    throw new FailedValidationException("skill not found");

                ProfileSkill skillDeletedFromProfile = (from ps in _dbContext.ProfileSkill
                                                        join p in _dbContext.Profile on ps.ProfileId equals p.ProfileId
                                                        join s in _dbContext.Skill on ps.SkillId equals s.SkillId
                                                        where s.SkillGuid == skillGuid && p.ProfileGuid == profileGuid && ps.IsDeleted == 1
                                                        select ps).FirstOrDefault();

                if (skillDeletedFromProfile != null)
                {
                    skillDeletedFromProfile.IsDeleted = 0;
                    skillDeletedFromProfile.ModifyDate = DateTime.UtcNow;
                    skillDeletedFromProfile.ModifyGuid = Guid.Empty;
                    _dbContext.ProfileSkill.Update(skillDeletedFromProfile);
                }
                else
                {
                    _dbContext.ProfileSkill.Add(new ProfileSkill()
                    {
                        CreateDate = DateTime.UtcNow,
                        CreateGuid = Guid.Empty,
                        IsDeleted = 0,
                        ProfileId = profileId,
                        SkillId = skillId,
                        ProfileSkillGuid = profileSkillGuid
                    });
                }
            }
            await _dbContext.SaveChangesAsync();
            return profileSkillGuids;
        }

        public async Task DeleteSkillsFromProfileForRecruiter(Guid subscriberGuid, List<Guid> profileSkillGuids)
        {
            bool isProfileAssociatedWithRecruiterCompany = (from ps in _dbContext.ProfileSkill
                                                            join p in _dbContext.Profile on ps.ProfileId equals p.ProfileId
                                                            join c in _dbContext.Company on p.CompanyId equals c.CompanyId
                                                            join rc in _dbContext.RecruiterCompany on c.CompanyId equals rc.CompanyId
                                                            join r in _dbContext.Recruiter on rc.RecruiterId equals r.RecruiterId
                                                            join s in _dbContext.Subscriber on r.SubscriberId equals s.SubscriberId
                                                            where s.SubscriberGuid == subscriberGuid && ps.ProfileSkillGuid == profileSkillGuids.FirstOrDefault()
                                                            select rc.RecruiterCompanyId).Any();
            if (!isProfileAssociatedWithRecruiterCompany)
                throw new FailedValidationException($"recruiter does not have permission to modify profile skills");

            foreach (Guid profileSkillGuid in profileSkillGuids)
            {
                var profileSkill = (from ps in _dbContext.ProfileSkill
                                       where ps.ProfileSkillGuid == profileSkillGuid && ps.IsDeleted == 0
                                       select ps).FirstOrDefault();
                if (profileSkill == null)
                    throw new NotFoundException($"profile skill '{profileSkillGuid}' not found");

                profileSkill.ModifyDate = DateTime.UtcNow;
                profileSkill.ModifyGuid = Guid.Empty;
                profileSkill.IsDeleted = 1;
                _dbContext.Update(profileSkill);
            }
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateProfileSkillsForRecruiter(Guid subscriberGuid, List<Guid> skillGuids, Guid profileGuid)
        {
            var recruiterSubscriber = new SqlParameter { ParameterName = "@RecruiterSubscriberGuid", SqlDbType = SqlDbType.UniqueIdentifier, Direction = ParameterDirection.Input, Value = (object)subscriberGuid ?? DBNull.Value };
            var profile = new SqlParameter { ParameterName = "@ProfileGuid", SqlDbType = SqlDbType.UniqueIdentifier, Direction = ParameterDirection.Input, Value = (object)profileGuid ?? DBNull.Value };
            DataTable skillsTable = new DataTable();
            skillsTable.Columns.Add("Guid", typeof(Guid));
            if (skillGuids != null && skillGuids.Count > 0)
            {
                foreach (var skillGuid in skillGuids)
                {
                    skillsTable.Rows.Add(skillGuid);
                }
            }
            var skills = new SqlParameter("@SkillGuids", skillsTable);
            skills.SqlDbType = SqlDbType.Structured;
            skills.TypeName = "dbo.GuidList";            
            var validationErrors = new SqlParameter { ParameterName = "@ValidationErrors", SqlDbType = SqlDbType.NVarChar, Size = -1, Direction = ParameterDirection.Output };
            var spParams = new object[] { recruiterSubscriber, profile, skills, validationErrors };

            var rowsAffected = _dbContext.Database.ExecuteSqlCommand(@"EXEC [G2].[System_Update_ProfileSkillsForRecruiter] @RecruiterSubscriberGuid, @ProfileGuid, @SkillGuids, @ValidationErrors OUTPUT", spParams);

            if (!string.IsNullOrWhiteSpace(validationErrors.Value.ToString()))
                throw new FailedValidationException(validationErrors.Value.ToString());
        }
    }
}