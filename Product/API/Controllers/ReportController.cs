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
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using UpDiddyLib.Dto;

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
                _db.SubscriberOfferActions
                .GroupBy(rca => new { rca.Action, rca.OfferName, rca.OfferCode })
                .Select(g => new OfferActionSummaryDto()
                {
                    Action = g.Key.Action,
                    ActionCount = g.Count(),
                    OfferName = g.Key.OfferName,
                    OfferCode = g.Key.OfferCode
                })
                .OrderByDescending(r => r.ActionCount)
                .ToList());
        }

        [HttpGet]
        [Route("/api/[controller]/recruiter-action-summary")]
        public async Task<IActionResult> RecruiterActionSummary()
        {
            return Ok(
                _db.RecruiterSubscriberActions
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
                .ToList());
        }

        [HttpGet]
        [Route("/api/[controller]/subscriber-action-summary")]
        public async Task<IActionResult> SubscriberActionSummary()
        {
            return Ok(
                _db.RecruiterSubscriberActions
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
                .ToList());
        }

        [HttpGet]
        [Route("/api/[controller]/subscribers")]
        public async Task<IActionResult> SubscribersReport([FromQuery] List<DateTime> dates)
        {
            List<BasicCountReportDto> totalsByDate = new List<BasicCountReportDto>();
            var subscriberQuery = _db.Subscriber.AsQueryable();
            var enrollmentQuery = _db.Enrollment.AsQueryable();

            BasicCountReportDto totals = new BasicCountReportDto()
            {
                SubscriberCount = subscriberQuery.Count(),
                EnrollmentCount = enrollmentQuery.Count()
            };

            if (!dates.Any())
                return Ok(new SubscriberReportDto()
                {
                    Totals = totals
                });

            dates.Sort();
            DateTime? prevDate = null;
            for (int i = dates.Count - 1; i >= 0; i--)
            {
                BasicCountReportDto bcr = new BasicCountReportDto();
                DateTime startDate = dates[i];
                subscriberQuery = _db.Subscriber.Where(s => s.CreateDate >= startDate);
                enrollmentQuery = _db.Enrollment.Where(s => s.DateEnrolled >= startDate);

                if (prevDate.HasValue)
                {
                    subscriberQuery = subscriberQuery.Where(s => s.CreateDate < prevDate);
                    enrollmentQuery = enrollmentQuery.Where(s => s.DateEnrolled < prevDate);

                    bcr.EndDate = prevDate.Value;
                }

                bcr.StartDate = startDate;
                bcr.SubscriberCount = subscriberQuery.Count();
                bcr.EnrollmentCount = enrollmentQuery.Count();
                totalsByDate.Add(bcr);

                prevDate = startDate;
            }

            return Ok(new SubscriberReportDto()
            {
                Totals = totals,
                Report = totalsByDate
            });
        }


        [HttpGet]
        [Route("/api/[controller]/partners")]
        public async Task<IActionResult> SubscriberReportByPartner([FromQuery] List<DateTime> dates)
        {
            var query = from s in _db.SubscriberSignUpPartnerReferences
                        join sub in _db.Subscriber on s.SubscriberId equals sub.SubscriberId
                        join p in _db.Partner on s.PartnerId equals p.PartnerId into pGroup
                        from partner in pGroup.DefaultIfEmpty()
                        join e in _db.Enrollment on s.SubscriberId equals e.SubscriberId into eGroup
                        from enrollment in eGroup.DefaultIfEmpty()
                        group new
                        {
                            PartnerName = partner == null ? "N/A" : partner.Name,
                            HasEnrollment = enrollment != null,
                            SubscriberId = s.SubscriberId
                        }
                        by (partner.PartnerId == null) ? -1 : partner.PartnerId into report
                        select new
                        {
                            subscriberCount = report.Select(x => x.SubscriberId).Distinct().Count(),
                            enrollmentCount = report.Count(x => x.HasEnrollment),
                            partnerName = report.First().PartnerName
                        };
            return Ok(new { report = query.ToList() });
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

            var actions = _db.Action.Select(x => new ActionKeyDto { Name = x.Name, ActionId = x.ActionId }).Where(x => x.ActionId == 6 || x.ActionId == 7).ToList();
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
    }
}