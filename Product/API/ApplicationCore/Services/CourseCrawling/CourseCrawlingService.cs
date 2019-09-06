using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyLib.Dto;
using Microsoft.EntityFrameworkCore;
using UpDiddyApi.Models;
using AutoMapper;
using UpDiddyApi.ApplicationCore.Services.CourseCrawling.Common;
using UpDiddyApi.ApplicationCore.Factory;
using Microsoft.Extensions.Configuration;
using UpDiddyApi.ApplicationCore.Interfaces;
using Microsoft.Extensions.Logging;
using Hangfire;
using UpDiddyApi.Workflow;

namespace UpDiddyApi.ApplicationCore.Services.CourseCrawling
{
    public class CourseCrawlingService : ICourseCrawlingService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        private readonly ISovrenAPI _sovrenApi;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CourseCrawlingService> _logger;

        public CourseCrawlingService(IRepositoryWrapper repositoryWrapper, IMapper mapper, IConfiguration configuration, ISovrenAPI sovrenApi, ILogger<CourseCrawlingService> logger)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _configuration = configuration;
            _sovrenApi = sovrenApi;
            _logger = logger;
        }

        public async Task<List<CourseSiteDto>> GetCourseSitesAsync()
        {
            var courseSites = await _repositoryWrapper.CourseSite.GetAllCourseSitesAsync();
            return _mapper.Map<List<CourseSiteDto>>(courseSites);
        }

        /// <summary>
        /// Performs validation, flags the course site to indicate it is being crawled, and invokes the course site crawl operation as a background job
        /// </summary>
        /// <param name="courseSiteGuid"></param>
        /// <returns></returns>
        public async Task<CourseSiteDto> CrawlCourseSiteAsync(Guid courseSiteGuid)
        {
            CourseSiteDto courseSiteDto = new CourseSiteDto() { CourseSiteGuid = courseSiteGuid };

            // load the course site
            var query = await _repositoryWrapper.CourseSite.GetByConditionAsync(x => x.CourseSiteGuid == courseSiteGuid);
            var courseSite = query.FirstOrDefault();

            // do nothing if the course site does not exist
            if (courseSite == null)
            {
                courseSiteDto.ValidationMessage = "Unrecognized course site; no action has been performed.";
                return courseSiteDto;
            }
            else
            {
                courseSiteDto.CourseSiteGuid = courseSite.CourseSiteGuid;
                courseSiteDto.Uri = courseSite.Uri;
            }

            // do not crawl the course site if another operation is pending
            if (courseSite.IsCrawling)
            {
                courseSiteDto.ValidationMessage = "Course site is currently being crawled; please wait until this operation is complete before initating another crawl.";
                return courseSiteDto;
            }
            else if (courseSite.IsSyncing)
            {
                courseSiteDto.ValidationMessage = "Course site is currently being synced; please wait until this operation is complete before initiating a crawl.";
                return courseSiteDto;
            }
            else
            {
                // mark the course site as being updated so nothing else can grab it
                courseSite.IsCrawling = true;
                _repositoryWrapper.CourseSite.Update(courseSite);
                await _repositoryWrapper.CourseSite.SaveAsync();

                // trigger the background job that will crawl the course site
                BackgroundJob.Enqueue<ScheduledJobs>(j => j.CrawlCourseSiteAsync(courseSite));
                courseSiteDto.IsCrawling = true;
                courseSiteDto.ValidationMessage = "Course site crawl was initiated.";
            }

            return courseSiteDto;
        }

        /// <summary>
        /// Performs validation, flags the course site to indicate it is being synced, and invokes the course site sync operation as a background job
        /// </summary>
        /// <param name="courseSiteGuid"></param>
        /// <returns></returns>
        public async Task<CourseSiteDto> SyncCourseSiteAsync(Guid courseSiteGuid)
        {
            CourseSiteDto courseSiteDto = new CourseSiteDto() { CourseSiteGuid = courseSiteGuid };

            // load the course site
            var query = await _repositoryWrapper.CourseSite.GetByConditionAsync(x => x.CourseSiteGuid == courseSiteGuid);
            var courseSite = query.FirstOrDefault();

            // do nothing if the course site does not exist
            if (courseSite == null)
            {
                courseSiteDto.ValidationMessage = "Unrecognized course site; no action has been performed.";
                return courseSiteDto;
            }
            else
            {
                courseSiteDto.CourseSiteGuid = courseSite.CourseSiteGuid;
                courseSiteDto.Uri = courseSite.Uri;
            }

            // do not sync the course site if another operation is pending
            if (courseSite.IsCrawling)
            {
                courseSiteDto.ValidationMessage = "Course site is currently being crawled; please wait until this operation is complete before initating the sync operation.";
                return courseSiteDto;
            }
            else if (courseSite.IsSyncing)
            {
                courseSiteDto.ValidationMessage = "Course site is currently being synced; please wait until this operation is complete before initiating another sync operation.";
                return courseSiteDto;
            }
            else
            {
                // mark the course site as being updated so nothing else can grab it
                courseSite.IsSyncing = true;
                _repositoryWrapper.CourseSite.Update(courseSite);
                await _repositoryWrapper.CourseSite.SaveAsync();

                // trigger the background job that will sync the course site
                BackgroundJob.Enqueue<ScheduledJobs>(j => j.SyncCourseSiteAsync(courseSite));
                courseSiteDto.IsSyncing = true;
                courseSiteDto.ValidationMessage = "Course site sync was initiated.";
            }

            return courseSiteDto;
        }
    }
}
