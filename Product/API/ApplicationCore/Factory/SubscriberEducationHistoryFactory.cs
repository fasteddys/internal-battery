using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;
namespace UpDiddyApi.ApplicationCore.Factory
{
    public class SubscriberEducationHistoryFactory
    {

        public static async Task<SubscriberEducationHistory> GetEducationHistoryByGuid(IRepositoryWrapper repositoryWrapper, Guid SubscriberEducationHistoryGuid)
        {
            return await repositoryWrapper.SubscriberEducationHistoryRepository.GetAllWithTracking()
                .Where(eh => eh.IsDeleted == 0 && eh.SubscriberEducationHistoryGuid == SubscriberEducationHistoryGuid)
                .FirstOrDefaultAsync();
        }


        public static async Task<SubscriberEducationHistory> GetEducationHistoryForSubscriber(IRepositoryWrapper repositoryWrapper, Subscriber subscriber, EducationalInstitution educationalInstitution, EducationalDegree educationalDegree, DateTime? startDate, DateTime? endDate, DateTime? degreeDate)
        {
            return await repositoryWrapper.SubscriberEducationHistoryRepository.GetAllWithTracking()
                .Where(eh => eh.IsDeleted == 0 && eh.EducationalDegreeId == educationalDegree.EducationalDegreeId &&
                    eh.EducationalInstitutionId == educationalInstitution.EducationalInstitutionId && eh.SubscriberId == subscriber.SubscriberId &&
                    eh.StartDate == startDate && eh.EndDate == endDate && eh.DegreeDate == degreeDate)
                .FirstOrDefaultAsync();
        }

        public static async Task<SubscriberEducationHistory> AddEducationHistoryForSubscriber(IRepositoryWrapper repositoryWrapper, Subscriber subscriber, SubscriberEducationHistoryDto educationkHistory,
                EducationalInstitution educationalInstitution, EducationalDegree educationalDegree, EducationalDegreeType educationalDegreeType)
        {
            SubscriberEducationHistory rVal = new SubscriberEducationHistory()
            {
                StartDate = educationkHistory.StartDate,
                EndDate = educationkHistory.EndDate,
                RelevantYear = (short?)GetMaxYear(new List<DateTime> {
                                       educationkHistory.StartDate ?? DateTime.MinValue,
                                       educationkHistory.EndDate ?? DateTime.MinValue,
                                       educationkHistory.DegreeDate ?? DateTime.MinValue
                }),
                SubscriberId = subscriber.SubscriberId,
                CreateDate = DateTime.UtcNow,
                CreateGuid = Guid.Empty,
                ModifyDate = DateTime.UtcNow,
                ModifyGuid = Guid.Empty,
                SubscriberEducationHistoryGuid = Guid.NewGuid(),
                IsDeleted = 0,
                EducationalInstitutionId = educationalInstitution.EducationalInstitutionId,
                DegreeDate = educationkHistory.DegreeDate,
                EducationalDegreeId = educationalDegree.EducationalDegreeId,
                EducationalDegreeTypeId = educationalDegreeType.EducationalDegreeTypeId
            };
            await repositoryWrapper.SubscriberEducationHistoryRepository.Create(rVal);
            await repositoryWrapper.SubscriberEducationHistoryRepository.SaveAsync();

            return rVal;
        }

    #region private methods
    private static int? GetMaxYear(List<DateTime> dateTimes)
    {
        var filteredDateTimes = dateTimes
              .Where(d => d != DateTime.MinValue && d != DateTime.MaxValue)
              .Select(d => d.Year)
              .ToList();

        return (filteredDateTimes != null && filteredDateTimes.Count > 0 ? filteredDateTimes.Max() : (int?)null);
    }
    #endregion
}
}
