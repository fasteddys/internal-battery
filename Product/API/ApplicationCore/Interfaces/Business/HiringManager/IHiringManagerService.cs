
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UpDiddyLib.Domain.Models;
using UpDiddyLib.Dto;


namespace UpDiddyApi.ApplicationCore.Interfaces.Business.HiringManager
{
    public interface IHiringManagerService
    {
        /// <summary>
        /// Adds hiring manager permission to AuthO.
        /// </summary>
        /// <param name="subscriberGuid"></param>
        /// <param name="nonBlocking"></param>
        /// <returns></returns>
        Task<bool> AddHiringManager(Guid subscriberGuid, bool nonBlocking = true);

        /// <summary>
        /// Gets hiring manager details.
        /// </summary>
        /// <param name="subscriberGuid"></param>
        /// <returns></returns>
        Task<HiringManagerDto> GetHiringManagerBySubscriberGuid(Guid subscriberGuid);

        /// <summary>
        /// Updates a hiring manager profile.
        /// </summary>
        /// <param name="subscriberGuid"></param>
        /// <param name="hiringManager"></param>
        /// <returns></returns>
        Task UpdateHiringManager(Guid subscriberGuid, HiringManagerDto hiringManager);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="candidateProfileGuid"></param>
        /// <returns></returns>
        Task<HiringManagerCandidateProfileDto> GetCandidateProfileDetail(Guid candidateProfileGuid);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="candidateProfileGuid"></param>
        /// <returns></returns>
        Task<EducationalHistoryDto> GetCandidateEducationHistory(Guid candidateProfileGuid, int limit, int offset, string sort, string order);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="candidateProfileGuid"></param>
        /// <returns></returns>
        Task<EmploymentHistoryDto> GetCandidateWorkHistory(Guid candidateProfileGuid, int limit, int offset, string sort, string order);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="candidateProfileGuid"></param>
        /// <returns></returns>
        Task<SkillListDto> GetCandidateSkills(Guid candidateProfileGuid, int limit, int offset, string sort, string order);

    }
}




