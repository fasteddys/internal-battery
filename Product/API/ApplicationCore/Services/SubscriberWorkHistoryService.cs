using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Factory;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.Models;
using UpDiddyApi.Workflow;
using UpDiddyLib.Dto;
using UpDiddyLib.Dto.Marketing;
using UpDiddyLib.Shared;
using UpDiddyLib.Helpers;
using System.Text.RegularExpressions;
using System.Web;
using AutoMapper;
using System.Security.Claims;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Http;
using UpDiddyApi.ApplicationCore.Exceptions;
using AutoMapper.QueryableExtensions;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class SubscriberWorkHistoryService : ISubscriberWorkHistoryService
    {
        private UpDiddyDbContext _db { get; set; }
        private IConfiguration _configuration { get; set; }
        private ICloudStorage _cloudStorage { get; set; }
        private IB2CGraph _graphClient { get; set; }
        private ILogger _logger { get; set; }
        private IRepositoryWrapper _repository { get; set; }
        private readonly IMapper _mapper;
        private ITaggingService _taggingService { get; set; }
        private IHangfireService _hangfireService { get; set; }

        public SubscriberWorkHistoryService(UpDiddyDbContext context,
            IConfiguration configuration,
            ICloudStorage cloudStorage,
            IB2CGraph graphClient,
            IRepositoryWrapper repository,
            ILogger<SubscriberService> logger,
            IMapper mapper,
            ITaggingService taggingService,
            IHangfireService hangfireService)
        {
            _db = context;
            _configuration = configuration;
            _cloudStorage = cloudStorage;
            _graphClient = graphClient;
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
            _taggingService = taggingService;
            _hangfireService = hangfireService;
        }

        public async Task<bool> AddWorkHistory(SubscriberWorkHistoryDto WorkHistoryDto, Guid subscriberGuid )
        {
            // sanitize user inputs
            WorkHistoryDto.Company = HttpUtility.HtmlEncode(WorkHistoryDto.Company);
            WorkHistoryDto.JobDescription = HttpUtility.HtmlEncode(WorkHistoryDto.JobDescription);
            WorkHistoryDto.Title = HttpUtility.HtmlEncode(WorkHistoryDto.Title);

            Subscriber subscriber = SubscriberFactory.GetSubscriberByGuid(_db, subscriberGuid);
            if (subscriber == null)
                throw new NotFoundException($"Subscriber {subscriberGuid} cannot be found");

            int companyId = -1;
            if (WorkHistoryDto.Company!= null)
            {
                Company company = CompanyFactory.GetOrAdd(_db, WorkHistoryDto.Company).Result;
                companyId = company != null ? company.CompanyId : -1;
            }

            int compensationTypeId = 0;
            if ( WorkHistoryDto.CompensationType != null )
            {
                CompensationType compensationType = CompensationTypeFactory.GetCompensationTypeByName(_db, WorkHistoryDto.CompensationType);

                if (compensationType == null)
                    compensationType = CompensationTypeFactory.GetOrAdd(_db, UpDiddyLib.Helpers.Constants.NotSpecifedOption);
                compensationTypeId = compensationType.CompensationTypeId;

            }

            SubscriberWorkHistory WorkHistory = new SubscriberWorkHistory()
            {
                SubscriberWorkHistoryGuid = Guid.NewGuid(),
                CreateGuid = Guid.Empty,
                ModifyGuid = Guid.Empty,
                CreateDate = DateTime.UtcNow,
                ModifyDate = DateTime.UtcNow,
                IsDeleted = 0,
                SubscriberId = subscriber.SubscriberId,
                StartDate = WorkHistoryDto.StartDate,
                EndDate = WorkHistoryDto.EndDate,
                IsCurrent = WorkHistoryDto.IsCurrent,
                Title = WorkHistoryDto.Title,
                JobDescription = WorkHistoryDto.JobDescription,
                Compensation = WorkHistoryDto.Compensation,
                CompensationTypeId = compensationTypeId,
                CompanyId = companyId
            };

            _db.SubscriberWorkHistory.Add(WorkHistory);
            _db.SaveChanges();

            // update google profile 
            _hangfireService.Enqueue<ScheduledJobs>(j => j.CloudTalentAddOrUpdateProfile(subscriber.SubscriberGuid.Value));

            return true;
        }



        public async Task<bool> UpdateEducationalHistory(SubscriberWorkHistoryDto WorkHistoryDto, Guid subscriberGuid, Guid workHistoryGuid)
        {

            // sanitize user inputs 
            WorkHistoryDto.Company = HttpUtility.HtmlEncode(WorkHistoryDto.Company);
            WorkHistoryDto.JobDescription = HttpUtility.HtmlEncode(WorkHistoryDto.JobDescription);
            WorkHistoryDto.Title = HttpUtility.HtmlEncode(WorkHistoryDto.Title);

            Subscriber subscriber = SubscriberFactory.GetSubscriberByGuid(_db, subscriberGuid);

            if (subscriber == null)
                throw new NotFoundException($"Subscriber {subscriberGuid} does not exist.");

            SubscriberWorkHistory WorkHistory = SubscriberWorkHistoryFactory.GetWorkHistoryByGuid(_db, workHistoryGuid);
            if (WorkHistory == null)
                throw new NotFoundException($"Work history {workHistoryGuid} does not exist.");


            if (WorkHistory.SubscriberId != subscriber.SubscriberId)
                throw new UnauthorizedAccessException();


            int companyId = -1;
            if ( WorkHistoryDto.Company != null )
            {
                 Company company = await CompanyFactory.GetOrAdd(_db, WorkHistoryDto.Company);
                 companyId = company != null ? company.CompanyId : -1;
            }

            int compensationTypeId = -1;
            if (WorkHistoryDto.CompensationType != null )
            {
                CompensationType compensationType = await CompensationTypeFactory.GetCompensationTypeByNameAsync(_db, WorkHistoryDto.CompensationType);

                if (compensationType != null)
                    compensationTypeId = compensationType.CompensationTypeId;
                else
                {
                    compensationType = CompensationTypeFactory.GetOrAdd(_db, UpDiddyLib.Helpers.Constants.NotSpecifedOption);
                    compensationTypeId = compensationType.CompensationTypeId;
                }
            }
                
            // Update the company ID
            WorkHistory.ModifyDate = DateTime.UtcNow;       
            WorkHistory.StartDate = WorkHistoryDto.StartDate;
            WorkHistory.EndDate = WorkHistoryDto.EndDate;
            WorkHistory.JobDescription = WorkHistoryDto.JobDescription;
            WorkHistory.Title = WorkHistoryDto.Title;
            WorkHistory.IsCurrent = WorkHistoryDto.IsCurrent;
            WorkHistory.Compensation = WorkHistoryDto.Compensation;

            if (compensationTypeId == -1)
                WorkHistory.CompensationTypeId = null;
            else
                WorkHistory.CompensationTypeId = compensationTypeId;


            if (companyId == -1)
                WorkHistory.CompanyId = null;
            else
                WorkHistory.CompanyId = companyId;
 
            _db.SaveChanges();
            
            // update google profile 
            _hangfireService.Enqueue<ScheduledJobs>(j => j.CloudTalentAddOrUpdateProfile(subscriber.SubscriberGuid.Value));

            return true;
        }


        public async Task<bool> DeleteWorklHistory(Guid subscriberGuid, Guid workHistoryGuid)
        {
            Subscriber subscriber = SubscriberFactory.GetSubscriberByGuid(_db, subscriberGuid);
            if ( subscriber == null )
                throw new NotFoundException($"Subscriber {subscriberGuid} not found.");

            SubscriberWorkHistory WorkHistory = SubscriberWorkHistoryFactory.GetWorkHistoryByGuid(_db, workHistoryGuid);
            if (WorkHistory == null )
                throw new NotFoundException($"Workhistory {workHistoryGuid} not found");

            if ( WorkHistory.SubscriberId != subscriber.SubscriberId)
                throw new UnauthorizedAccessException();

            // Soft delete of the workhistory item
            WorkHistory.ModifyDate = DateTime.UtcNow;
            WorkHistory.IsDeleted = 1;
            _db.SaveChanges();
            
            return true;
        }

        public async Task<List<SubscriberWorkHistoryDto>> GetWorkHistory(Guid subscriberGuid)
        {

            Subscriber subscriber = SubscriberFactory.GetSubscriberByGuid(_db, subscriberGuid);
            if (subscriber == null)
                throw new NotFoundException($"Subscriber {subscriberGuid} not found.");

            var workHistory = _db.SubscriberWorkHistory
            .Where(s => s.IsDeleted == 0 && s.SubscriberId == subscriber.SubscriberId)
            .OrderByDescending(s => s.StartDate)
            .Select(wh => new SubscriberWorkHistory()
            {
                Company = new Company()
                {
                    CompanyGuid = wh.Company.CompanyGuid,
                    CompanyId = wh.Company.CompanyId,
                    CompanyName = HttpUtility.HtmlDecode(wh.Company.CompanyName),
                    CreateDate = wh.Company.CreateDate,
                    CreateGuid = wh.Company.CreateGuid,
                    IsDeleted = wh.Company.IsDeleted,
                    ModifyDate = wh.Company.ModifyDate,
                    ModifyGuid = wh.Company.ModifyGuid
                },
                CompanyId = wh.CompanyId,
                Compensation = wh.Compensation,
                CompensationType = wh.CompensationType,
                CompensationTypeId = wh.CompensationTypeId,
                CreateDate = wh.CreateDate,
                CreateGuid = wh.CreateGuid,
                EndDate = wh.EndDate,
                IsCurrent = wh.IsCurrent,
                IsDeleted = wh.IsDeleted,
                JobDescription = HttpUtility.HtmlDecode(wh.JobDescription),
                ModifyDate = wh.ModifyDate,
                ModifyGuid = wh.ModifyGuid,
                StartDate = wh.StartDate,
                SubscriberId = wh.SubscriberId,
                SubscriberWorkHistoryGuid = wh.SubscriberWorkHistoryGuid,
                SubscriberWorkHistoryId = wh.SubscriberWorkHistoryId,
                Title = HttpUtility.HtmlDecode(wh.Title)
            // ignoring subscriber property
        })
            .ProjectTo<SubscriberWorkHistoryDto>(_mapper.ConfigurationProvider)
            .ToList();

            return workHistory;
        }






    }
}
