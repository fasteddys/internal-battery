using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
namespace UpDiddyApi.ApplicationCore.Factory
{
    public class JobPostingSkillFactory
    {

        public static async Task<List<JobPostingSkill>> GetSkillsForPosting(IRepositoryWrapper repositoryWrapper, int jobPostingId)
        {
            return await repositoryWrapper.JobPostingSkillRepository.GetAllWithTracking()
                   .Include(s => s.Skill)
                   .Where(s => s.JobPostingId == jobPostingId)
                   .ToListAsync();
        }

        public static async Task DeleteSkillsForPosting(IRepositoryWrapper repositoryWrapper, int jobPostingId)
        {
            List<JobPostingSkill> theList = await repositoryWrapper.JobPostingSkillRepository.GetAllWithTracking()
                   .Where(s => s.JobPostingId == jobPostingId)
                   .ToListAsync();

            foreach (JobPostingSkill jps in theList)
                repositoryWrapper.JobPostingSkillRepository.Delete(jps);
            await repositoryWrapper.JobPostingSkillRepository.SaveAsync();
        }


        /// <summary>
        /// Create a job posting skill 
        /// </summary>
        /// <param name="repositoryWrapper"></param>
        /// <param name="jobPostingId"></param>
        /// <param name="skillGuid"></param>
        /// <returns></returns>
        public static async Task<JobPostingSkill> CreateJobPostingSkill(IRepositoryWrapper repositoryWrapper, int jobPostingId, Guid skillGuid)
        {
            Skill skill = await SkillFactory.GetSkillByGuid(repositoryWrapper, skillGuid);
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
                JobPostingId = jobPostingId,
                JobPostingSkillGuid = skill.SkillGuid
            };

        }

        /// <summary>
        /// Add skill to jobposting 
        /// </summary>
        /// <param name="jobPostingId"></param>
        /// <param name="skillGuid"></param>
        /// <returns></returns>
        public static async Task<bool> Add(IRepositoryWrapper repositoryWrapper, int jobPostingId, Guid skillGuid)
        {
            // todo make this more efficient 
            try
            {
                JobPostingSkill postingSkill = await CreateJobPostingSkill(repositoryWrapper, jobPostingId, skillGuid);
                await repositoryWrapper.JobPostingSkillRepository.Create(postingSkill);
                await repositoryWrapper.JobPostingSkillRepository.SaveAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}
