using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;
namespace UpDiddyApi.ApplicationCore.Factory
{
    public class SubscriberWorkHistoryFactory
    {

        public static async Task<SubscriberWorkHistory> GetWorkHistoryByGuid(IRepositoryWrapper repositoryWrapper, Guid SubcriberWorkHistoryGuid)
        {
            return await repositoryWrapper.SubscriberWorkHistoryRepository.GetAll()
                .Where(wh => wh.IsDeleted == 0 && wh.SubscriberWorkHistoryGuid == SubcriberWorkHistoryGuid )
                .FirstOrDefaultAsync();
        }

        public static async Task<SubscriberWorkHistory> GetWorkHistoryForSubscriber(IRepositoryWrapper repositoryWrapper, Subscriber subscriber, Company company, DateTime? startDate, DateTime? endDate)
        {
            return await repositoryWrapper.SubscriberWorkHistoryRepository.GetAll()
                .Where(wh => wh.IsDeleted == 0 && wh.CompanyId == company.CompanyId && wh.SubscriberId == subscriber.SubscriberId && wh.StartDate == startDate && wh.EndDate == endDate)
                .FirstOrDefaultAsync();
        }

        public static async Task<SubscriberWorkHistory> AddWorkHistoryForSubscriber(IRepositoryWrapper repositoryWrapper, Subscriber subscriber, SubscriberWorkHistoryDto workHistory, Company company)
        {

            // html encode title and job description to be consistent with api endpoints that encode to protect again script injection 
            try
            {
                SubscriberWorkHistory wh = new SubscriberWorkHistory()
                {
                    StartDate = workHistory.StartDate,
                    EndDate = workHistory.EndDate,
                    CompanyId = company.CompanyId,
                    SubscriberId = subscriber.SubscriberId,
                    Title = HttpUtility.HtmlEncode(workHistory.Title),
                    JobDescription = HttpUtility.HtmlEncode(workHistory.JobDescription),
                    IsCurrent = workHistory.IsCurrent,
                    CreateDate = DateTime.UtcNow,
                    CreateGuid = Guid.Empty,
                    ModifyDate = DateTime.UtcNow,
                    ModifyGuid = Guid.Empty,
                    SubscriberWorkHistoryGuid = Guid.NewGuid(),
                    IsDeleted = 0
                };
                await repositoryWrapper.SubscriberWorkHistoryRepository.Create(wh);
                await repositoryWrapper.SubscriberWorkHistoryRepository.SaveAsync();
                return wh;
            }
            catch
            {
                return null;
            }
        

        }
    }
}
