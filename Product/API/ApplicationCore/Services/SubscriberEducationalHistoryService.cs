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
    public class SubscriberEducationalHistoryService : ISubscriberEducationalHistoryService
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

        public SubscriberEducationalHistoryService(UpDiddyDbContext context,
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

        // todo jab start with update Educational History 

        public async Task<bool> CreateEducationalHistory(SubscriberEducationHistoryDto EducationHistoryDto, Guid subscriberGuid)
        {
            // sanitize user inputs
            EducationHistoryDto.EducationalDegree = HttpUtility.HtmlEncode(EducationHistoryDto.EducationalDegree);
            EducationHistoryDto.EducationalInstitution = HttpUtility.HtmlEncode(EducationHistoryDto.EducationalInstitution);
 
            Subscriber subscriber = SubscriberFactory.GetSubscriberByGuid(_db, subscriberGuid);
            if (subscriber == null)
                throw new NotFoundException($"Subscriber {subscriberGuid} does not exist");
            // Find or create the institution 
            EducationalInstitution educationalInstitution = EducationalInstitutionFactory.GetOrAdd(_db, EducationHistoryDto.EducationalInstitution).Result;
            int educationalInstitutionId = educationalInstitution.EducationalInstitutionId;
            // Find or create the degree major 
            EducationalDegree educationalDegree = EducationalDegreeFactory.GetOrAdd(_db, EducationHistoryDto.EducationalDegree).Result;
            int educationalDegreeId = educationalDegree.EducationalDegreeId;
            // Find or create the degree type 
            EducationalDegreeType educationalDegreeType = EducationalDegreeTypeFactory.GetEducationalDegreeTypeByDegreeType(_db, EducationHistoryDto.EducationalDegreeType);
            int educationalDegreeTypeId = 0;
            if (educationalDegreeType == null)
                educationalDegreeType = EducationalDegreeTypeFactory.GetOrAdd(_db, UpDiddyLib.Helpers.Constants.NotSpecifedOption).Result;
            educationalDegreeTypeId = educationalDegreeType.EducationalDegreeTypeId;

            SubscriberEducationHistory EducationHistory = new SubscriberEducationHistory()
            {
                SubscriberEducationHistoryGuid = Guid.NewGuid(),
                CreateGuid = Guid.Empty,
                ModifyGuid = Guid.Empty,
                CreateDate = DateTime.UtcNow,
                ModifyDate = DateTime.UtcNow,
                IsDeleted = 0,
                SubscriberId = subscriber.SubscriberId,
                StartDate = EducationHistoryDto.StartDate,
                EndDate = EducationHistoryDto.EndDate,
                DegreeDate = EducationHistoryDto.DegreeDate,
                EducationalDegreeId = educationalDegreeId,
                EducationalDegreeTypeId = educationalDegreeTypeId,
                EducationalInstitutionId = educationalInstitutionId
            };

            _db.SubscriberEducationHistory.Add(EducationHistory);
            _db.SaveChanges();

            // update google profile 
            _hangfireService.Enqueue<ScheduledJobs>(j => j.CloudTalentAddOrUpdateProfile(subscriber.SubscriberGuid.Value));

            return true;
        }

        public async Task<bool> UpdateEducationalHistory(SubscriberEducationHistoryDto EducationHistoryDto, Guid subscriberGuid, Guid educationalHistoryGuid)
        {

            EducationHistoryDto.EducationalDegree = HttpUtility.HtmlEncode(EducationHistoryDto.EducationalDegree);
            EducationHistoryDto.EducationalInstitution = HttpUtility.HtmlEncode(EducationHistoryDto.EducationalInstitution);
   
            Subscriber subscriber = SubscriberFactory.GetSubscriberByGuid(_db, subscriberGuid);
            if (subscriber == null)
                throw new NotFoundException($"Subscriber {subscriberGuid} does not exist"); 

            SubscriberEducationHistory EducationHistory = SubscriberEducationHistoryFactory.GetEducationHistoryByGuid(_db, educationalHistoryGuid);
            if (EducationHistory == null )
                throw new NotFoundException($"Educational History  {EducationHistoryDto.SubscriberEducationHistoryGuid} does not exist");

            if (EducationHistory.SubscriberId != subscriber.SubscriberId)
                throw new UnauthorizedAccessException();

            // Find or create the institution 
            EducationalInstitution educationalInstitution =  await EducationalInstitutionFactory.GetOrAdd(_db, EducationHistoryDto.EducationalInstitution);
            int educationalInstitutionId = educationalInstitution.EducationalInstitutionId;
            // Find or create the degree major 
            EducationalDegree educationalDegree = await EducationalDegreeFactory.GetOrAdd(_db, EducationHistoryDto.EducationalDegree);
            int educationalDegreeId = educationalDegree.EducationalDegreeId;
            // Find or create the degree type 
            EducationalDegreeType educationalDegreeType =  await EducationalDegreeTypeFactory.GetEducationalDegreeTypeByDegreeTypeAsync(_db, EducationHistoryDto.EducationalDegreeType);
            int educationalDegreeTypeId = 0;
            if (educationalDegreeType == null)
                educationalDegreeType = await EducationalDegreeTypeFactory.GetOrAdd(_db, UpDiddyLib.Helpers.Constants.NotSpecifedOption);
            educationalDegreeTypeId = educationalDegreeType.EducationalDegreeTypeId;

            EducationHistory.ModifyDate = DateTime.UtcNow;
            EducationHistory.StartDate = EducationHistoryDto.StartDate;
            EducationHistory.EndDate = EducationHistoryDto.EndDate;
            EducationHistory.DegreeDate = EducationHistoryDto.DegreeDate;
            EducationHistory.EducationalDegreeId = educationalDegreeId;
            EducationHistory.EducationalDegreeTypeId = educationalDegreeTypeId;
            EducationHistory.EducationalInstitutionId = educationalInstitutionId;
            _db.SaveChanges();

            // update google profile 
            _hangfireService.Enqueue<ScheduledJobs>(j => j.CloudTalentAddOrUpdateProfile(subscriber.SubscriberGuid.Value));
 
            return true;
        }



        public async Task<bool> DeleteEducationalHistory(Guid subscriberGuid, Guid educationalHistoryGuid)
        {
 
            Subscriber subscriber = SubscriberFactory.GetSubscriberByGuid(_db, subscriberGuid);
            SubscriberEducationHistory EducationHistory = SubscriberEducationHistoryFactory.GetEducationHistoryByGuid(_db, educationalHistoryGuid);

            if (EducationHistory == null )
               throw new NotFoundException($"Educational History {educationalHistoryGuid} does not exist");

            if (EducationHistory.SubscriberId != subscriber.SubscriberId)
               throw new UnauthorizedAccessException();

            // Soft delete of the workhistory item
            EducationHistory.IsDeleted = 1;
            _db.SaveChanges();
 
            return true;
        }


        public async Task<List<SubscriberEducationHistoryDto>> GetEducationalHistory(Guid subscriberGuid)
        {
            
            Subscriber subscriber = SubscriberFactory.GetSubscriberByGuid(_db, subscriberGuid);
            if (subscriber == null)
                throw new NotFoundException($"Subscriber {subscriberGuid} does not exist");

            var educationHistory = _db.SubscriberEducationHistory
            .Where(s => s.IsDeleted == 0 && s.SubscriberId == subscriber.SubscriberId)
            .OrderByDescending(s => s.StartDate)
            .Select(eh => new SubscriberEducationHistory()
            {
                CreateDate = eh.CreateDate,
                CreateGuid = eh.CreateGuid,
                DegreeDate = eh.DegreeDate,
                EducationalDegree = new EducationalDegree()
                {
                    CreateDate = eh.EducationalDegree.CreateDate,
                    CreateGuid = eh.EducationalDegree.CreateGuid,
                    Degree = HttpUtility.HtmlDecode(eh.EducationalDegree.Degree),
                    EducationalDegreeGuid = eh.EducationalDegree.EducationalDegreeGuid,
                    EducationalDegreeId = eh.EducationalDegree.EducationalDegreeId,
                    IsDeleted = eh.EducationalDegree.IsDeleted,
                    ModifyDate = eh.EducationalDegree.ModifyDate,
                    ModifyGuid = eh.EducationalDegree.ModifyGuid
                },
                EducationalDegreeId = eh.EducationalDegreeId,
                EducationalDegreeType = eh.EducationalDegreeType,         
                EducationalDegreeTypeId = eh.EducationalDegreeTypeId,
                EducationalInstitution = new EducationalInstitution()
                {
                    CreateDate = eh.EducationalInstitution.CreateDate,
                    CreateGuid = eh.EducationalInstitution.CreateGuid,
                    EducationalInstitutionGuid = eh.EducationalInstitution.EducationalInstitutionGuid,
                    EducationalInstitutionId = eh.EducationalInstitution.EducationalInstitutionId,
                    IsDeleted = eh.EducationalInstitution.IsDeleted,
                    ModifyDate = eh.EducationalInstitution.ModifyDate,
                    ModifyGuid = eh.EducationalInstitution.ModifyGuid,
                    Name = HttpUtility.HtmlDecode(eh.EducationalInstitution.Name)
                },
                EducationalInstitutionId = eh.EducationalInstitutionId,
                EndDate = eh.EndDate,
                IsDeleted = eh.IsDeleted,
                ModifyDate = eh.ModifyDate,
                ModifyGuid = eh.ModifyGuid,
                StartDate = eh.StartDate,
                SubscriberEducationHistoryGuid = eh.SubscriberEducationHistoryGuid,
                SubscriberEducationHistoryId = eh.SubscriberEducationHistoryId,
                SubscriberId = eh.SubscriberId
                // ignoring Subscriber property
            })
            .ProjectTo<SubscriberEducationHistoryDto>(_mapper.ConfigurationProvider)
            .ToList();

            return educationHistory;
        }









    }
}
