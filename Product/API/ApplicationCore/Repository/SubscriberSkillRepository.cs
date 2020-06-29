using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using UpDiddyLib.Domain.Models;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class SubscriberSkillRepository : UpDiddyRepositoryBase<SubscriberSkill>, ISubscriberSkillRepository
    {
        private readonly UpDiddyDbContext _dbContext;
        public SubscriberSkillRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<SubscriberSkill> GetBySubscriberGuidAndSkillGuid(Guid subscriberGuid, Guid skillGuid)
        {
            return await (from ss in _dbContext.SubscriberSkill
                          join s in _dbContext.Subscriber on ss.SubscriberId equals s.SubscriberId
                          join sk in _dbContext.Skill on ss.SkillId equals sk.SkillId
                          where sk.SkillGuid == skillGuid && s.SubscriberGuid == subscriberGuid
                          select ss).FirstOrDefaultAsync();
        }

        public async Task<List<SubscriberSkill>> GetActiveSkillsBySubscriberGuid(Guid subscriberGuid)
        {
            return await (from ss in _dbContext.SubscriberSkill
                          join s in _dbContext.Subscriber on ss.SubscriberId equals s.SubscriberId
                          join sk in _dbContext.Skill on ss.SkillId equals sk.SkillId
                          where s.SubscriberGuid == subscriberGuid && ss.IsDeleted == 0
                          select ss).Include(x => x.Skill).ToListAsync();
        }

         public async Task<List<SubscriberSkill>> GetAllSkillsBySubscriberGuid(Guid subscriberGuid)
        {
            return await (from ss in _dbContext.SubscriberSkill
                          join s in _dbContext.Subscriber on ss.SubscriberId equals s.SubscriberId
                          join sk in _dbContext.Skill on ss.SkillId equals sk.SkillId
                          where s.SubscriberGuid == subscriberGuid 
                          select ss).Include(x => x.Skill).ToListAsync();
        }

        public async Task UpdateCandidateSkills(Guid subscriberGuid, List<string> skillNames)
        {
            var subscriber = new SqlParameter { ParameterName = "@SubscriberGuid", SqlDbType = SqlDbType.UniqueIdentifier, Direction = ParameterDirection.Input, Value = (object)subscriberGuid ?? DBNull.Value };
            DataTable skillsTable = new DataTable();
            skillsTable.Columns.Add("string", typeof(string));
            if (skillNames != null && skillNames.Count > 0)
            {
                foreach (var skillName in skillNames)
                {
                    skillsTable.Rows.Add(skillName);
                }
            }
            var skills = new SqlParameter("@SkillNames", skillsTable);
            skills.SqlDbType = SqlDbType.Structured;
            skills.TypeName = "dbo.StringList";
            var spParams = new object[] { subscriber, skills };

            var rowsAffected = _dbContext.Database.ExecuteSqlCommand(@"EXEC [dbo].[System_Update_CandidateSkills] @SubscriberGuid, @SkillNames", spParams);
        }

        public async Task<List<SkillDto>> GetCandidateSkills(Guid subscriberGuid, int limit, int offset, string sort, string order)
        {
            var spParams = new object[] {
                new SqlParameter("@SubscriberGuid", subscriberGuid),
                new SqlParameter("@Limit", limit),
                new SqlParameter("@Offset", offset),
                new SqlParameter("@Sort", sort),
                new SqlParameter("@Order", order),
                };
            List<SkillDto> skills = null;
            skills = await _dbContext.Skills.FromSql<SkillDto>("[dbo].[System_Get_CandidateSkills] @SubscriberGuid, @Limit, @Offset, @Sort, @Order", spParams).ToListAsync();
            return skills;
        }
    }
}
