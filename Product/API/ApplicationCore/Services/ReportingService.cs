using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyLib.Dto;
using UpDiddyLib.Dto.Reporting;
using Microsoft.AspNet.OData.Query;
using static UpDiddyLib.Helpers.Constants;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class ReportingService : IReportingService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        public ReportingService(IRepositoryWrapper repositoryWrapper)
        {
            _repositoryWrapper = repositoryWrapper;
        }
        public async Task<List<JobApplicationCountDto>> GetApplicationCountByCompanyAsync(ODataQueryOptions<JobApplication> options, Guid? companyGuid)
        {
            //get all jobs querayble
            var queryable = options.ApplyTo( _repositoryWrapper.JobApplication.GetAllJobApplicationsAsync());
            var jobPostingRep =  _repositoryWrapper.JobPosting.GetAllJobPostings();
            //get all companies queryable
            var companyRep =  _repositoryWrapper.Company.GetAllCompanies();

            var jobApplicationsQuery = from jobApplication in queryable.Cast<JobApplication>()
                                       join jp in jobPostingRep on jobApplication.JobPostingId equals jp.JobPostingId
                                       join company in companyRep on jp.CompanyId equals company.CompanyId
                                       where !companyGuid.HasValue || company.CompanyGuid == companyGuid.Value
                                       group new
                                       {
                                           CompanyName = company.CompanyName,
                                           CompanyGuid = company.CompanyGuid
                                       } by jp.CompanyId into g
                                       select new JobApplicationCountDto
                                       {
                                           CompanyName = g.First().CompanyName,
                                           CompanyGuid = g.First().CompanyGuid,
                                           ApplicationCount = g.Count()
                                       };
            var list = await jobApplicationsQuery.ToListAsync();
            return list;
        }

        public async Task<List<NotificationCountsReportDto>> GetReadNotificationsAsync(ODataQueryOptions<Notification> options)
        {
            var view =  _repositoryWrapper.NotificationRepository.GetNotificationReadCounts();
            return await view.Select(v => new NotificationCountsReportDto()
            {
                NotificationTitle = v.NotificationTitle,
                PublishedDate = v.PublishedDate,
                ReadCount = v.ReadCount
            })
            .OrderByDescending(n => n.PublishedDate)
            .ToListAsync();
        }

        /// <summary>
        /// Get active/published job counts per company and posted date range.
        /// </summary>
        /// <param name="startPostDate"></param>
        /// <param name="endPostDate"></param>
        /// <returns>A list of DTOs representing a job posting per company by date report.</returns>
        public async Task<List<JobPostingCountReportDto>> GetActiveJobPostCountPerCompanyByDates(DateTime? startPostDate, DateTime? endPostDate)
        {
            List<JobPostingCountReportDto> jobPostingCountReportDtos;

            //get all jobs querayble
            var jobPostingRepo = _repositoryWrapper.JobPosting.GetAllJobPostings();

            //get all companies queryable
            var companyRepo = _repositoryWrapper.Company.GetAllCompanies();

            var query = from c in companyRepo
                        join jp in jobPostingRepo on c.CompanyId equals jp.CompanyId
                        where jp.JobStatus == (int)JobPostingStatus.Active
                            && c.IsJobPoster == 1
                            && (startPostDate == null || (jp.PostingDateUTC.Date >= startPostDate))
                            && (endPostDate == null || (jp.PostingDateUTC.Date <= endPostDate))
                        group new { c.CompanyName, jp.PostingDateUTC }
                        by new { c.CompanyName, jp.PostingDateUTC.Date } into g
                        orderby g.Key.CompanyName, g.Key.Date
                        select new JobPostingCountReportDto
                        {
                            CompanyName = g.Key.CompanyName,
                            PostingDate = g.Key.Date,
                            PostingCount = g.Select(x => x.PostingDateUTC.Date).Count()
                        };

            jobPostingCountReportDtos = await query.ToListAsync();

            return jobPostingCountReportDtos;
        }

        public async Task<List<JobViewCountDto>> GetJobViewCount(Guid jobPostingGuid)
        {

            List<UpDiddyApi.Models.JobPosting> jobPostingList = new List<UpDiddyApi.Models.JobPosting>();
            UpDiddyApi.Models.JobPosting jobPosting = new Models.JobPosting();

            if (jobPostingGuid == null)
            {
                var jobPostingListQuerable = _repositoryWrapper.JobPosting.GetAllJobPostings();
                jobPostingList.AddRange(jobPostingListQuerable.ToList());
            }
            else
            {
                jobPosting = await _repositoryWrapper.JobPosting.GetJobPostingByGuid(jobPostingGuid);
                jobPostingList.Add(jobPosting);
            }


            var entityType = await _repositoryWrapper.EntityTypeRepository.GetByNameAsync(EventType.JobPosting);
            var subscriberActionList = await _repositoryWrapper.SubscriberActionRepository.GetSubscriberActionByEntityAndEntityType(entityType.EntityTypeId, jobPostingGuid == null ? null : jobPosting?.JobPostingId);

            var ActionsOnJobsList = subscriberActionList.Join(jobPostingList, sa => sa.EntityId, jp => jp.JobPostingId, (sa, jp) => new
            {
                JobPostingGuid = jp.JobPostingGuid,
                JobName = jp.Title,
                SubscriberActionId = sa.SubscriberActionId
            });

            var jobviewCountDtoList = ActionsOnJobsList.GroupBy(aojl => aojl.JobPostingGuid)
                                .Select(x => new JobViewCountDto
                                {
                                    JobPostingGuid = x.First().JobPostingGuid,
                                    JobName = x.First().JobName,
                                    Count = x.Count()
                                }).ToList();


            return jobviewCountDtoList;
        }

        public async Task<List<JobAbandonmentStatistics>> GetJobAbandonmentCountByDateAsync(DateTime startDate, DateTime endDate)
        {
            var result = await _repositoryWrapper.StoredProcedureRepository.GetJobAbandonmentStatisticsAsync(startDate, endDate);
            return result;        
        }

        public async Task<List<SubscriberSignUpCourseEnrollmentStatistics>> GetSubscriberSignUpCourseEnrollmentStatisticsAsync(DateTime? startDate, DateTime? endDate)
        {
            var result = await _repositoryWrapper.StoredProcedureRepository.GetSubscriberSignUpCourseEnrollmentStatisticsAsync(startDate, endDate);
            return result;
        }

        public async Task<SubscriberReportDto> GetSubscriberAndEnrollmentReportByDates(List<DateTime> dates)
        {
            List<BasicCountReportDto> totalsByDate = new List<BasicCountReportDto>();

            BasicCountReportDto totals = new BasicCountReportDto()
            {
                SubscriberCount = await _repositoryWrapper.SubscriberRepository.GetSubscribersCountByStartEndDates(),
                EnrollmentCount = await _repositoryWrapper.EnrollmentRepository.GetEnrollmentsCountByStartEndDates()
            };

            if (!dates.Any())
                return new SubscriberReportDto()
                {
                    Totals = totals
                };

            dates.Sort();
            DateTime? prevDate = null;
            for (int i = dates.Count - 1; i >= 0; i--)
            {
                DateTime startDate = dates[i];

                var subscribersCount = await _repositoryWrapper.SubscriberRepository.GetSubscribersCountByStartEndDates(startDate, prevDate);
                var enrollmentsCount = await _repositoryWrapper.EnrollmentRepository.GetEnrollmentsCountByStartEndDates(startDate, prevDate);

                BasicCountReportDto basicCountReport = new BasicCountReportDto()
                {
                    StartDate = startDate,
                    EndDate = prevDate.HasValue ? prevDate : null,
                    SubscriberCount = subscribersCount,
                    EnrollmentCount = enrollmentsCount
                };

                totalsByDate.Add(basicCountReport);

                prevDate = startDate;
            }

            return new SubscriberReportDto()
            {
                Totals = totals,
                Report = totalsByDate
            };
        }
    }
}
