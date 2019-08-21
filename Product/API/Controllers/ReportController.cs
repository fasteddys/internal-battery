using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.Models;
using UpDiddyLib.Dto.Reporting;
using Microsoft.AspNet.OData.Query;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace UpDiddyApi.Controllers
{
    [Authorize(Policy = "IsCareerCircleAdmin")]
    public class ReportController : Controller
    {
        private UpDiddyDbContext _db { get; set; }
        private readonly IReportingService _reportingService;
        private readonly ILogger _syslog;
        public ReportController(UpDiddyDbContext db, IReportingService reportingService, ILogger<ReportController> sysLog)
        {
            _db = db;
            _reportingService = reportingService;
            _syslog = sysLog;
        }

        [HttpGet]
        [Route("/api/[controller]/offer-action-summary")]
        public async Task<IActionResult> OfferActionSummary()
        {
            return Ok(
                await _db.SubscriberOfferActions
                .GroupBy(rca => new { rca.Action, rca.OfferName, rca.OfferCode })
                .Select(g => new OfferActionSummaryDto()
                {
                    Action = g.Key.Action,
                    ActionCount = g.Count(),
                    OfferName = g.Key.OfferName,
                    OfferCode = g.Key.OfferCode
                })
                .OrderByDescending(r => r.ActionCount)
                .ToListAsync());
        }

        [HttpGet]
        [Route("/api/[controller]/recruiter-action-summary")]
        public async Task<IActionResult> RecruiterActionSummary()
        {
            return Ok(
                await _db.RecruiterSubscriberActions
                .GroupBy(rca => new { rca.RecruiterEmail, rca.RecruiterFirstName, rca.RecruiterLastName, rca.Action })
                .Select(g => new RecruiterActionSummaryDto()
                {
                    Action = g.Key.Action,
                    ActionCount = g.Count(),
                    RecruiterEmail = g.Key.RecruiterEmail,
                    RecruiterFirstName = g.Key.RecruiterFirstName,
                    RecruiterLastName = g.Key.RecruiterLastName,
                })
                .OrderByDescending(r => r.ActionCount)
                .ToListAsync());
        }

        [HttpGet]
        [Route("/api/[controller]/subscriber-action-summary")]
        public async Task<IActionResult> SubscriberActionSummary()
        {
            return Ok(
                await _db.RecruiterSubscriberActions
                .GroupBy(rca => new { rca.SubscriberEmail, rca.SubscriberFirstName, rca.SubscriberLastName, rca.Action })
                .Select(g => new SubscriberActionSummaryDto()
                {
                    Action = g.Key.Action,
                    ActionCount = g.Count(),
                    SubscriberEmail = g.Key.SubscriberEmail,
                    SubscriberFirstName = g.Key.SubscriberFirstName,
                    SubscriberLastName = g.Key.SubscriberLastName,
                })
                .OrderByDescending(r => r.ActionCount)
                .ToListAsync());
        }

        [HttpGet]
        [Route("/api/[controller]/subscribers")]
        public async Task<IActionResult> SubscribersReport([FromQuery] List<DateTime> dates)
        {
            ActionResult response;
            try
            {
                var subscriberAndEnrollmentReportByDates = await _reportingService.GetSubscriberAndEnrollmentReportByDates(dates);
                response = Ok(subscriberAndEnrollmentReportByDates);
                return response;
            }
            catch (Exception ex)
            {
                _syslog.Log(LogLevel.Error, $"Error in ReportController.SubscribersReport method for dates={JsonConvert.SerializeObject(dates)}", ex);
                response = StatusCode(500);
                return response;
            }
        }


        [HttpGet]
        [Route("/api/[controller]/partners")]
        public async Task<IActionResult> SubscriberReportByPartner([FromQuery] List<DateTime> dates)
        {
            ActionResult response;
            try
            {
                var subscriberSignUpCourseEnrollmentStatistics = await _reportingService.GetSubscriberSignUpCourseEnrollmentStatisticsAsync();
                response = Ok(new { report = subscriberSignUpCourseEnrollmentStatistics });
                return response;
            }
            catch (Exception ex)
            {
                _syslog.Log(LogLevel.Error, $"Error in ReportController.SubscriberReportByPartner method", ex);
                response = StatusCode(500);
                return response;
            }
        }

        [HttpGet]
        [Route("/api/[controller]/subscriber-actions")]
        public async Task<IActionResult> SAReportAsync(ODataQueryOptions<SubscriberAction> options)
        {
            var queryable = options.ApplyTo(_db.SubscriberAction.AsQueryable());

            var query = from sa in queryable.Cast<SubscriberAction>()
                        join partRef in _db.SubscriberSignUpPartnerReferences on sa.EntityId equals partRef.SubscriberId
                        join p in _db.Partner on partRef.PartnerId equals p.PartnerId into pGroup
                        from partner in pGroup.DefaultIfEmpty()
                        join a in _db.Action on sa.ActionId equals a.ActionId
                        group new
                        {
                            PartnerName = partner == null ? "N/A" : partner.Name,
                            ActionId = sa.ActionId,
                            ActionName = a.Name
                        } by (partner.PartnerId == null) ? -1 : partner.PartnerId into report
                        select new PartnerStatsDto
                        {
                            PartnerName = report.First().PartnerName,
                            Stats = report.GroupBy(x => x.ActionId).Select(y => new
                            {
                                ActionId = y.First().ActionId,
                                Count = y.Count()
                            }).ToDictionary(x => x.ActionId.ToString(), x => x.Count)
                        };

            var actions = await _db.Action.Select(x => new ActionKeyDto { Name = x.Name, ActionId = x.ActionId }).Where(x => x.ActionId == 6 || x.ActionId == 7).ToListAsync();
            return Ok(new { report = query, actionKey = actions });
        }

        /// <summary>
        /// Get Job Application Count by Company, StartDate and EndDate
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("/api/[controller]/job-applications")]
        [Route("/api/[controller]/job-applications/company/{companyGuid?}")]
        public async Task<IActionResult> JobApplicationsAsync(ODataQueryOptions<JobApplication> options, Guid? companyGuid = null)
        {
            ActionResult response;
            try
            {
                if (ModelState.IsValid)
                {
                    var jobApplicationCountDtoList = await _reportingService.GetApplicationCountByCompanyAsync(options, companyGuid);
                    response = Ok(jobApplicationCountDtoList);

                }
                else
                    response = BadRequest();
            }
            catch (Exception ex)
            {
                _syslog.LogError(ex, $"Error in ReportController.JobApplicationsAsync method for CompanyGuid={companyGuid.Value}");
                response = StatusCode(500);
            }

            return response;
        }

        /// <summary>
        /// Get read notifications by start date and end date
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("/api/[controller]/notification-reads")]
        public async Task<IActionResult> ReadNotificationsAsync(ODataQueryOptions<Notification> options)
        {
            ActionResult response;
            try
            {
                if (ModelState.IsValid)
                {
                    var notificationsCountsReportDto = await _reportingService.GetReadNotificationsAsync(options);
                    response = Ok(notificationsCountsReportDto);

                }
                else
                    response = BadRequest();
            }
            catch (Exception ex)
            {
                _syslog.LogError(ex, $"Error in ReportController.ReadNotificationsAsync method");
                response = StatusCode(500);
            }

            return response;
        }

        /// <summary>
        /// Get active/published job counts per company and posted date range.
        /// </summary>
        /// <param name="startPostDate"></param>
        /// <param name="endPostDate"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("/api/[controller]/job-post-count/{startPostDate?}/{endPostDate?}")]
        public async Task<IActionResult> ActiveJobPostCountPerCompanyByDates(DateTime? startPostDate = null, DateTime? endPostDate = null)
        {
            ActionResult response;
            try
            {
                if (ModelState.IsValid)
                {
                    var jobPostingCountReportDtos = await _reportingService.GetActiveJobPostCountPerCompanyByDates(startPostDate, endPostDate);
                    response = Ok(jobPostingCountReportDtos);

                }
                else
                    response = BadRequest();
            }
            catch (Exception ex)
            {
                _syslog.LogError(ex, $"Error in ReportController.ActiveJobPostCountPerCompanyByDates method for StartPostDate={startPostDate} and EndPostDate={endPostDate}");
                response = StatusCode(500);
            }

            return response;
        }

        /// <summary>
        /// Get Job View Count
        /// </summary>
        /// <param name="jobPostingGuid"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("/api/[controller]/job-view-count/{jobPostingGuid?}")]
        public async Task<IActionResult> JobViewCount(Guid jobPostingGuid)
        {
            ActionResult response;
            try
            {
                if (ModelState.IsValid)
                {
                    var jobViewCountDtoList = await _reportingService.GetJobViewCount(jobPostingGuid);
                    response = Ok(jobViewCountDtoList);

                }
                else
                    response = BadRequest();
            }
            catch (Exception ex)
            {
                _syslog.LogError(ex, $"Error in ReportController.JobViewCount method for jobPostingGuid={jobPostingGuid}");
                response = StatusCode(500);
            }

            return response;
        }

        [HttpGet]
        [Route("api/[controller]/job-abandonment-count/{startDate?}/{endDate?}")]
        public async Task<IActionResult> JobAbandonmentCountByDateAsync(DateTime startDate, DateTime endDate)
        {
           ActionResult response;
            try
            {
                if (ModelState.IsValid)
                {
                    var result = await _reportingService.GetJobAbandonmentCountByDateAsync(startDate, endDate);
                    response = Ok(result);

                }
                else
                    response = BadRequest();
            }
            catch (Exception ex)
            {
                _syslog.LogError(ex, $"Error in ReportController.JobAbandonmentCountByDateAsync method for StartDate={startDate} and EndDate={endDate}");
                response = StatusCode(500);
            }

            return response;
        }

    }
}