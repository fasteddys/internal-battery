using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Factory
{
    public class JobPostingSkillFactory
    {

        public static List<JobPostingSkill> GetSkillsForPosting(UpDiddyDbContext db, int jobPostingId )
        {
            return  db.JobPostingSkill
                   .Where(s => s.JobPostingId == jobPostingId)
                   .ToList();
        }

        public static void DeleteSkillsForPosting(UpDiddyDbContext db, int jobPostingId)
        {
            List<JobPostingSkill> theList =  db.JobPostingSkill
                   .Where(s => s.JobPostingId == jobPostingId)
                   .ToList();

            foreach ( JobPostingSkill jps in theList)
                 db.JobPostingSkill.Remove(jps);
            db.SaveChanges();
        }





        /// <summary>
        /// Create a job posting skill 
        /// </summary>
        /// <param name="db"></param>
        /// <param name="jobPostingId"></param>
        /// <param name="skillGuid"></param>
        /// <returns></returns>
        public static JobPostingSkill CreateJobPostingSkill(UpDiddyDbContext db, int jobPostingId, Guid skillGuid)
        {
            Skill skill = SkillFactory.GetSkillByGuid(db, skillGuid);
            if (skill == null)
                throw new Exception($"{skillGuid} is not a valid skill");

            return new JobPostingSkill()
            {
                CreateDate = DateTime.Now,
                ModifyDate = DateTime.Now,
                IsDeleted = 0,
                CreateGuid = Guid.Empty,
                ModifyGuid = Guid.Empty,
                SkillId = skill.SkillId,
                JobPostingId = jobPostingId                
            };

        }

        /// <summary>
        /// Add skill to jobposting 
        /// </summary>
        /// <param name="jobPostingId"></param>
        /// <param name="skillGuid"></param>
        /// <returns></returns>
        public static bool Add(UpDiddyDbContext db, int jobPostingId, Guid skillGuid)
        {
            // todo make this more efficient 
            try
            {
                JobPostingSkill postingSkill = CreateJobPostingSkill(db,jobPostingId,skillGuid);
                db.JobPostingSkill.Add(postingSkill);
                db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}
