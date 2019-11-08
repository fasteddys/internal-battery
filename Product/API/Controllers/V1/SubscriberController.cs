﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using UpDiddyApi.Authorization;
using AutoMapper;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyLib.Dto.User;
using UpDiddyLib.Helpers;
using System.IO;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Factory;
using System.Data.SqlClient;
using AutoMapper.QueryableExtensions;
using System.Data;
using System.Web;
using UpDiddyLib.Dto.Marketing;
using UpDiddyLib.Shared;
using Hangfire;
using Microsoft.AspNetCore.Http;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using X.PagedList;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Workflow;
using UpDiddyApi.ApplicationCore.Services;
using System.Net.Http;
using System.Net;
using Auth0.ManagementApi;
using Auth0.ManagementApi.Models;
using Newtonsoft.Json.Linq;
using UpDiddyApi.ApplicationCore.Services.Identity.Interfaces;

[Route("api/[controller]")]
public class SubscriberController : Controller
{
    private readonly UpDiddyDbContext _db = null;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;
    private readonly ILogger _syslog;
    private readonly IDistributedCache _cache;
    private IAuthorizationService _authorizationService;
    private ISubscriberService _subscriberService;
    private ISubscriberNotificationService _subscriberNotificationService;
    private ICloudStorage _cloudStorage;
    private ISysEmail _sysEmail;
    private IJobService _jobService;
    private readonly IRepositoryWrapper _repositoryWrapper;
    private ITaggingService _taggingService;
    private readonly ICloudTalentService _cloudTalentService;
    private readonly IHangfireService _hangfireService;
    private readonly IJobPostingService _jobPostingService;
    private readonly ManagementApiClient _managementApiClient;
    private readonly IUserService _userService;
    private readonly IFileDownloadTrackerService _fileDownloadTrackerService;

    public SubscriberController(UpDiddyDbContext db,
        IMapper mapper,
        IConfiguration configuration,
        ILogger<SubscriberController> sysLog,
        IDistributedCache distributedCache,
        ICloudStorage cloudStorage,
        ISysEmail sysEmail,
        IAuthorizationService authorizationService,
        IRepositoryWrapper repositoryWrapper,
        ISubscriberService subscriberService,
        ITaggingService taggingService,
        ISubscriberNotificationService subscriberNotificationService,
        IJobService jobService,
        IHttpClientFactory httpClientFactory,
        IHangfireService hangfireService,
        IJobPostingService jobPostingService,
        IUserService userService,
        IFileDownloadTrackerService fileDownloadTrackerService,
        ICloudTalentService cloudTalentService)
    {
        _db = db;
        _mapper = mapper;
        _configuration = configuration;
        _cache = distributedCache;
        _syslog = sysLog;
        _cloudStorage = cloudStorage;
        _sysEmail = sysEmail;
        _authorizationService = authorizationService;
        _repositoryWrapper = repositoryWrapper;
        _subscriberService = subscriberService;
        _subscriberNotificationService = subscriberNotificationService;
        _jobService = jobService;
        _taggingService = taggingService;
        _cloudTalentService = cloudTalentService;
        _hangfireService = hangfireService;
        _jobPostingService = jobPostingService;
        _userService = userService;
        _fileDownloadTrackerService = fileDownloadTrackerService;
    }

    #region Basic Subscriber Endpoints

    [HttpPut("/api/[controller]/{subscriberGuid}/update-last-sign-in")]
    public IActionResult UpdateLastSignIn(Guid subscriberGuid)
    {
        if (subscriberGuid == null || subscriberGuid == Guid.Empty)
            return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = "Invalid subscriber" });

        SubscriberFactory.UpdateLastSignIn(_db, subscriberGuid);

        return Ok(new BasicResponseDto() { StatusCode = 200, Description = "Subscriber updated successfully" });
    }


    [HttpGet("{subscriberGuid}/company")]
    [Authorize]
    public async Task<IActionResult> GetCompanies(Guid subscriberGuid)
    {
        // Validate guid for GetSubscriber call
        if (Guid.Empty.Equals(subscriberGuid) || subscriberGuid == null)
            return NotFound();

        Subscriber subscriber = await SubscriberFactory.GetSubscriberByGuid(_repositoryWrapper, subscriberGuid);
        if (subscriber == null)
        {
            return NotFound(new { code = 404, message = $"Subscriber {subscriberGuid} not found" });
        }

        List<RecruiterCompany> companies = await RecruiterCompanyFactory.GetRecruiterCompanyById(_repositoryWrapper, subscriber.SubscriberId);
        return Ok(_mapper.Map<List<RecruiterCompanyDto>>(companies));
    }

    [HttpGet("{subscriberGuid}")]
    public async Task<IActionResult> Get(Guid subscriberGuid)

    {
        // Validate guid for GetSubscriber call
        if (Guid.Empty.Equals(subscriberGuid) || subscriberGuid == null)
            return NotFound();

        Guid loggedInUserGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
        var isAuth = await _authorizationService.AuthorizeAsync(User, "IsRecruiterPolicy");

        if (subscriberGuid == loggedInUserGuid || isAuth.Succeeded)
        {

            SubscriberDto subscriberDto = await SubscriberFactory.GetSubscriber(_repositoryWrapper, subscriberGuid, _syslog, _mapper);

            if (subscriberDto == null)
                return Ok(subscriberDto);

            // track the subscriber action if performed by someone other than the user who owns the file
            if (loggedInUserGuid != subscriberDto.SubscriberGuid.Value)
                new SubscriberActionFactory(_repositoryWrapper, _db, _configuration, _syslog, _cache).TrackSubscriberAction(loggedInUserGuid, "View subscriber", "Subscriber", subscriberDto.SubscriberGuid);

            return Ok(subscriberDto);
        }
        else
            return Unauthorized();
    }

    [Authorize(Policy = "IsCareerCircleAdmin")]
    [HttpDelete("{subscriberGuid}/{cloudIdentifier}")]
    public IActionResult DeleteSubscriber(Guid subscriberGuid, Guid cloudIdentifier)
    {
        try
        {
            if (subscriberGuid == null)
                return BadRequest(new { code = 400, message = "No subscriber identifier was provided" });

            var subscriber = _db.Subscriber.Where(s => s.SubscriberGuid == subscriberGuid).FirstOrDefault();
            if (subscriber == null)
                return BadRequest(new { code = 404, message = "No subscriber could be found with that identifier" });

            // delete the Auth0 account associated with the subscriber
            var getUserResponse = _userService.GetUserByEmailAsync(subscriber.Email).Result;
            _userService.DeleteUserAsync(getUserResponse.User.UserId);

            // perform logical delete on the subscriber entity only
            subscriber.IsDeleted = 1;
            subscriber.ModifyDate = DateTime.UtcNow;
            subscriber.ModifyGuid = Guid.Empty;
            _db.SaveChanges();

            // write audit information to the database
            var deleteUserAuditInformation = new JObject();
            deleteUserAuditInformation.Add("action", "deleted");
            deleteUserAuditInformation.Add("Auth0UserId", subscriber.Auth0UserId);
            SubscriberProfileStagingStoreFactory.Save(_db, subscriber, "Auth0", "Json", JsonConvert.SerializeObject(deleteUserAuditInformation));
            // delete subscriber from cloud talent 
            _hangfireService.Enqueue<ScheduledJobs>(j => j.CloudTalentDeleteProfile(subscriberGuid, cloudIdentifier));
        }
        catch (Exception e)
        {
            _syslog.Log(LogLevel.Error, $"SubscriberController.DeleteSubscriber:: An error occured while attempting to delete the subscriber. Message: {e.Message}", e);
            return StatusCode(500, false);
        }

        return Ok(true);
    }

    [HttpPut("/api/[controller]")]
    public IActionResult Update([FromBody] SubscriberDto Subscriber)
    {
        Guid subsriberGuidClaim = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
        if (subsriberGuidClaim != Subscriber.SubscriberGuid)
            return Unauthorized();

        var subscriberGuid = new SqlParameter("@SubscriberGuid", Subscriber.SubscriberGuid);
        var firstName = new SqlParameter("@FirstName", (object)Subscriber.FirstName ?? DBNull.Value);
        var lastName = new SqlParameter("@LastName", (object)Subscriber.LastName ?? DBNull.Value);
        var address = new SqlParameter("@Address", (object)Subscriber.Address ?? DBNull.Value);
        var city = new SqlParameter("@City", (object)Subscriber.City ?? DBNull.Value);
        var postalCode = new SqlParameter("@PostalCode", (object)Subscriber.PostalCode ?? (object)DBNull.Value);
        var stateGuid = new SqlParameter("@StateGuid", (Subscriber?.State?.StateGuid != null ? (object)Subscriber.State.StateGuid : DBNull.Value));
        var phoneNumber = new SqlParameter("@PhoneNumber", (object)Subscriber.PhoneNumber ?? (object)DBNull.Value);
        var facebookUrl = new SqlParameter("@FacebookUrl", (object)Subscriber.FacebookUrl ?? DBNull.Value);
        var twitterUrl = new SqlParameter("@TwitterUrl", (object)Subscriber.TwitterUrl ?? DBNull.Value);
        var linkedInUrl = new SqlParameter("@LinkedInUrl", (object)Subscriber.LinkedInUrl ?? DBNull.Value);
        var stackOverflowUrl = new SqlParameter("@StackOverflowUrl", (object)Subscriber.StackOverflowUrl ?? DBNull.Value);
        var gitHubUrl = new SqlParameter("@GitHubUrl", (object)Subscriber.GithubUrl ?? DBNull.Value);


        DataTable table = new DataTable();
        table.Columns.Add("Guid", typeof(Guid));
        if (Subscriber.Skills != null)
        {
            foreach (var skill in Subscriber.Skills)
            {
                table.Rows.Add(skill.SkillGuid);
            }
        }

        var skillGuids = new SqlParameter("@SkillGuids", table);
        skillGuids.SqlDbType = SqlDbType.Structured;
        skillGuids.TypeName = "dbo.GuidList";

        var spParams = new object[] { subscriberGuid, firstName, lastName, address, city, postalCode, stateGuid, phoneNumber, facebookUrl, twitterUrl, linkedInUrl, stackOverflowUrl, gitHubUrl, skillGuids };

        var rowsAffected = _db.Database.ExecuteSqlCommand(@"
                EXEC [dbo].[System_Update_Subscriber] 
                    @SubscriberGuid,
                    @FirstName,
	                @LastName,
	                @Address,
	                @City,
                    @PostalCode,
	                @StateGuid,
                    @PhoneNumber,
	                @FacebookUrl,
	                @TwitterUrl,
	                @LinkedInUrl,
	                @StackOverflowUrl,
	                @GithubUrl,
	                @SkillGuids", spParams);


        // update google profile 
        _hangfireService.Enqueue<ScheduledJobs>(j => j.CloudTalentAddOrUpdateProfile(Subscriber.SubscriberGuid.Value));


        return Ok();
    }
    #endregion

    #region Subscriber Work History
    [Authorize]
    [HttpGet]
    [Route("/api/[controller]/{subscriberGuid}/work-history")]
    public async Task<IActionResult> GetWorkHistoryAsync(Guid subscriberGuid)
    {
        Guid loggedInUserGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
        var isAuth = await _authorizationService.AuthorizeAsync(User, "IsRecruiterPolicy");

        if (subscriberGuid != loggedInUserGuid && !isAuth.Succeeded)
            return Unauthorized();

        Subscriber subscriber = await SubscriberFactory.GetSubscriberByGuid(_repositoryWrapper, subscriberGuid);
        if (subscriber == null)
            return BadRequest();

        var workHistory = _repositoryWrapper.SubscriberWorkHistoryRepository.GetAllWithTracking()
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

        return Ok(workHistory);
    }

    [Authorize]
    [HttpPost]
    [Route("/api/[controller]/{subscriberGuid}/work-history")]
    // TODO looking into consolidating Add and Update to reduce code redundancy
    public async Task<IActionResult> AddWorkHistory(Guid subscriberGuid, [FromBody] SubscriberWorkHistoryDto WorkHistoryDto)
    {
        // sanitize user inputs
        WorkHistoryDto.Company = HttpUtility.HtmlEncode(WorkHistoryDto.Company);
        WorkHistoryDto.JobDescription = HttpUtility.HtmlEncode(WorkHistoryDto.JobDescription);
        WorkHistoryDto.Title = HttpUtility.HtmlEncode(WorkHistoryDto.Title);

        Guid loggedInUserGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);

        if (subscriberGuid != loggedInUserGuid)
            return Unauthorized();

        Subscriber subscriber = await SubscriberFactory.GetSubscriberByGuid(_repositoryWrapper, subscriberGuid);
        if (subscriber == null)
            return BadRequest();
        Company company = CompanyFactory.GetOrAdd(_repositoryWrapper, WorkHistoryDto.Company).Result;
        int companyId = company != null ? company.CompanyId : -1;
        CompensationType compensationType = await CompensationTypeFactory.GetCompensationTypeByName(_repositoryWrapper, WorkHistoryDto.CompensationType);
        int compensationTypeId = 0;
        if (compensationType == null)
            compensationType = await CompensationTypeFactory.GetOrAdd(_repositoryWrapper, Constants.NotSpecifedOption);
        compensationTypeId = compensationType.CompensationTypeId;

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

        await _repositoryWrapper.SubscriberWorkHistoryRepository.Create(WorkHistory);
        await _repositoryWrapper.SaveAsync();

        // update google profile 
        _hangfireService.Enqueue<ScheduledJobs>(j => j.CloudTalentAddOrUpdateProfile(subscriber.SubscriberGuid.Value));


        return Ok(_mapper.Map<SubscriberWorkHistoryDto>(WorkHistory));
    }

    [Authorize]
    [HttpPut]
    [Route("/api/[controller]/{subscriberGuid}/work-history")]
    public async Task<IActionResult> UpdateWorkHistory(Guid subscriberGuid, [FromBody] SubscriberWorkHistoryDto WorkHistoryDto)
    {
        // sanitize user inputs 
        WorkHistoryDto.Company = HttpUtility.HtmlEncode(WorkHistoryDto.Company);
        WorkHistoryDto.JobDescription = HttpUtility.HtmlEncode(WorkHistoryDto.JobDescription);
        WorkHistoryDto.Title = HttpUtility.HtmlEncode(WorkHistoryDto.Title);

        Guid loggedInUserGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);

        if (subscriberGuid != loggedInUserGuid)
            return Unauthorized();

        Subscriber subscriber = await SubscriberFactory.GetSubscriberByGuid(_repositoryWrapper, subscriberGuid);
        Company company = CompanyFactory.GetOrAdd(_repositoryWrapper, WorkHistoryDto.Company).Result;
        int companyId = company != null ? company.CompanyId : -1;
        CompensationType compensationType = await CompensationTypeFactory.GetCompensationTypeByName(_repositoryWrapper, WorkHistoryDto.CompensationType);
        int compensationTypeId = 0;
        if (compensationType != null)
            compensationTypeId = compensationType.CompensationTypeId;
        else
        {
            compensationType = await CompensationTypeFactory.GetOrAdd(_repositoryWrapper, Constants.NotSpecifedOption);
            compensationTypeId = compensationType.CompensationTypeId;
        }

        if (subscriber == null)
            return BadRequest();

        SubscriberWorkHistory WorkHistory = await SubscriberWorkHistoryFactory.GetWorkHistoryByGuid(_repositoryWrapper, WorkHistoryDto.SubscriberWorkHistoryGuid);
        if (WorkHistory == null || WorkHistory.SubscriberId != subscriber.SubscriberId)
            return BadRequest();

        // Update the company ID
        WorkHistory.ModifyDate = DateTime.UtcNow;
        WorkHistory.CompanyId = companyId;
        WorkHistory.StartDate = WorkHistoryDto.StartDate;
        WorkHistory.EndDate = WorkHistoryDto.EndDate;
        WorkHistory.JobDescription = WorkHistoryDto.JobDescription;
        WorkHistory.Title = WorkHistoryDto.Title;
        WorkHistory.IsCurrent = WorkHistoryDto.IsCurrent;
        WorkHistory.Compensation = WorkHistoryDto.Compensation;
        WorkHistory.CompensationTypeId = compensationTypeId;
        await _repositoryWrapper.SaveAsync();

        // update google profile 
        _hangfireService.Enqueue<ScheduledJobs>(j => j.CloudTalentAddOrUpdateProfile(subscriber.SubscriberGuid.Value));

        return Ok(_mapper.Map<SubscriberWorkHistoryDto>(WorkHistory));
    }

    [Authorize]
    [HttpDelete]
    [Route("/api/[controller]/{subscriberGuid}/work-history/{WorkHistoryGuid}")]
    public async Task<IActionResult> DeleteWorkHistory(Guid subscriberGuid, Guid WorkHistoryGuid)
    {
        Guid loggedInUserGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);

        if (subscriberGuid != loggedInUserGuid)
            return Unauthorized();

        Subscriber subscriber = await SubscriberFactory.GetSubscriberByGuid(_repositoryWrapper, subscriberGuid);
        SubscriberWorkHistory WorkHistory = await SubscriberWorkHistoryFactory.GetWorkHistoryByGuid(_repositoryWrapper, WorkHistoryGuid);
        if (WorkHistory == null || WorkHistory.SubscriberId != subscriber.SubscriberId)
            return BadRequest();
        // Soft delete of the workhistory item
        WorkHistory.IsDeleted = 1;
        await _repositoryWrapper.SaveAsync();

        return Ok(_mapper.Map<SubscriberWorkHistoryDto>(WorkHistory));
    }
    #endregion

    #region Subscriber Education History
    [Authorize]
    [HttpGet]
    [Route("/api/[controller]/{subscriberGuid}/education-history")]
    public async Task<IActionResult> GetEducationHistoryAsync(Guid subscriberGuid)
    {
        Guid loggedInUserGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
        var isAuth = await _authorizationService.AuthorizeAsync(User, "IsRecruiterPolicy");

        if (subscriberGuid != loggedInUserGuid && !isAuth.Succeeded)
            return Unauthorized();

        Subscriber subscriber = await SubscriberFactory.GetSubscriberByGuid(_repositoryWrapper, subscriberGuid);
        if (subscriber == null)
            return BadRequest();

        var educationHistory = _repositoryWrapper.SubscriberEducationHistoryRepository.GetAllWithTracking()
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
            //new EducationalDegreeType()
            //{
            //    CreateDate = eh.EducationalDegreeType.CreateDate,
            //    CreateGuid = eh.EducationalDegreeType.CreateGuid,
            //    DegreeType = eh.EducationalDegreeType.DegreeType,
            //    EducationalDegreeTypeGuid = eh.EducationalDegreeType.EducationalDegreeTypeGuid,
            //    EducationalDegreeTypeId = eh.EducationalDegreeType.EducationalDegreeTypeId,
            //    IsDeleted = eh.EducationalDegreeType.IsDeleted,
            //    ModifyDate = eh.EducationalDegreeType.ModifyDate,
            //    ModifyGuid = eh.EducationalDegreeType.ModifyGuid
            //},
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

        return Ok(educationHistory);
    }

    [Authorize]
    [HttpPost]
    [Route("/api/[controller]/{subscriberGuid}/education-history")]
    // TODO looking into consolidating Add and Update to reduce code redundancy
    public async Task<IActionResult> AddEducationalHistory(Guid subscriberGuid, [FromBody] SubscriberEducationHistoryDto EducationHistoryDto)
    {
        // sanitize user inputs
        EducationHistoryDto.EducationalDegree = HttpUtility.HtmlEncode(EducationHistoryDto.EducationalDegree);
        EducationHistoryDto.EducationalInstitution = HttpUtility.HtmlEncode(EducationHistoryDto.EducationalInstitution);

        Guid loggedInUserGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);

        if (subscriberGuid != loggedInUserGuid)
            return Unauthorized();

        Subscriber subscriber = await SubscriberFactory.GetSubscriberByGuid(_repositoryWrapper, subscriberGuid);
        if (subscriber == null)
            return BadRequest();
        // Find or create the institution 
        EducationalInstitution educationalInstitution = await EducationalInstitutionFactory.GetOrAdd(_repositoryWrapper, EducationHistoryDto.EducationalInstitution);
        int educationalInstitutionId = educationalInstitution.EducationalInstitutionId;
        // Find or create the degree major 
        EducationalDegree educationalDegree = await EducationalDegreeFactory.GetOrAdd(_repositoryWrapper, EducationHistoryDto.EducationalDegree);
        int educationalDegreeId = educationalDegree.EducationalDegreeId;
        // Find or create the degree type 
        EducationalDegreeType educationalDegreeType = await EducationalDegreeTypeFactory.GetEducationalDegreeTypeByDegreeType(_repositoryWrapper, EducationHistoryDto.EducationalDegreeType);
        int educationalDegreeTypeId = 0;
        if (educationalDegreeType == null)
            educationalDegreeType = await EducationalDegreeTypeFactory.GetOrAdd(_repositoryWrapper, Constants.NotSpecifedOption);
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

        await _repositoryWrapper.SubscriberEducationHistoryRepository.Create(EducationHistory);
        await _repositoryWrapper.SaveAsync();

        // update google profile 
        _hangfireService.Enqueue<ScheduledJobs>(j => j.CloudTalentAddOrUpdateProfile(subscriber.SubscriberGuid.Value));

        return Ok(_mapper.Map<SubscriberEducationHistoryDto>(EducationHistory));
    }

    [Authorize]
    [HttpPut]
    [Route("/api/[controller]/{subscriberGuid}/education-history")]
    public async Task<IActionResult> UpdateEducationHistory(Guid subscriberGuid, [FromBody] SubscriberEducationHistoryDto EducationHistoryDto)
    {
        // sanitize user inputs
        EducationHistoryDto.EducationalDegree = HttpUtility.HtmlEncode(EducationHistoryDto.EducationalDegree);
        EducationHistoryDto.EducationalInstitution = HttpUtility.HtmlEncode(EducationHistoryDto.EducationalInstitution);

        Guid loggedInUserGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);

        if (subscriberGuid != loggedInUserGuid)
            return Unauthorized();

        Subscriber subscriber = await SubscriberFactory.GetSubscriberByGuid(_repositoryWrapper, subscriberGuid);
        if (subscriber == null)
            return BadRequest();

        SubscriberEducationHistory EducationHistory = await SubscriberEducationHistoryFactory.GetEducationHistoryByGuid(_repositoryWrapper, EducationHistoryDto.SubscriberEducationHistoryGuid);
        if (EducationHistory == null || EducationHistory.SubscriberId != subscriber.SubscriberId)
            return BadRequest();
        // Find or create the institution 
        EducationalInstitution educationalInstitution = EducationalInstitutionFactory.GetOrAdd(_repositoryWrapper, EducationHistoryDto.EducationalInstitution).Result;
        int educationalInstitutionId = educationalInstitution.EducationalInstitutionId;
        // Find or create the degree major 
        EducationalDegree educationalDegree = EducationalDegreeFactory.GetOrAdd(_repositoryWrapper, EducationHistoryDto.EducationalDegree).Result;
        int educationalDegreeId = educationalDegree.EducationalDegreeId;
        // Find or create the degree type 
        EducationalDegreeType educationalDegreeType = await EducationalDegreeTypeFactory.GetEducationalDegreeTypeByDegreeType(_repositoryWrapper, EducationHistoryDto.EducationalDegreeType);
        int educationalDegreeTypeId = 0;
        if (educationalDegreeType == null)
            educationalDegreeType = EducationalDegreeTypeFactory.GetOrAdd(_repositoryWrapper, Constants.NotSpecifedOption).Result;
        educationalDegreeTypeId = educationalDegreeType.EducationalDegreeTypeId;

        EducationHistory.ModifyDate = DateTime.UtcNow;
        EducationHistory.StartDate = EducationHistoryDto.StartDate;
        EducationHistory.EndDate = EducationHistoryDto.EndDate;
        EducationHistory.DegreeDate = EducationHistoryDto.DegreeDate;
        EducationHistory.EducationalDegreeId = educationalDegreeId;
        EducationHistory.EducationalDegreeTypeId = educationalDegreeTypeId;
        EducationHistory.EducationalInstitutionId = educationalInstitutionId;
        await _repositoryWrapper.SaveAsync();

        // update google profile 
        _hangfireService.Enqueue<ScheduledJobs>(j => j.CloudTalentAddOrUpdateProfile(subscriber.SubscriberGuid.Value));

        return Ok(_mapper.Map<SubscriberEducationHistoryDto>(EducationHistory));
    }

    [Authorize]
    [HttpDelete]
    [Route("/api/[controller]/{subscriberGuid}/education-history/{EducationHistoryGuid}")]
    public async Task<IActionResult> DeleteEducationHistory(Guid subscriberGuid, Guid EducationHistoryGuid)
    {
        Guid loggedInUserGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);

        if (subscriberGuid != loggedInUserGuid)
            return Unauthorized();

        Subscriber subscriber = await SubscriberFactory.GetSubscriberByGuid(_repositoryWrapper, subscriberGuid);
        SubscriberEducationHistory EducationHistory = await SubscriberEducationHistoryFactory.GetEducationHistoryByGuid(_repositoryWrapper, EducationHistoryGuid);
        if (EducationHistory == null || EducationHistory.SubscriberId != subscriber.SubscriberId)
            return BadRequest();
        // Soft delete of the workhistory item
        EducationHistory.IsDeleted = 1;
        await _repositoryWrapper.SaveAsync();

        return Ok(_mapper.Map<SubscriberEducationHistory>(EducationHistory));
    }
    #endregion

    /// <summary>
    /// Avatar upload endpoint 
    /// </summary>  
    /// <returns></returns>
    [Authorize]
    [HttpPost]
    [Route("/api/[controller]/avatar")]
    public IActionResult UploadAvatar(IFormFile avatar)
    {
        Guid subscriberGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
        string errorMsg = string.Empty;

        if (SubscriberFactory.UpdateAvatar(_repositoryWrapper, _configuration, avatar, subscriberGuid, ref errorMsg))
            return Ok(new BasicResponseDto() { StatusCode = 200, Description = "Avatar updated." });
        else
            return Ok(new BasicResponseDto() { StatusCode = 400, Description = errorMsg });
    }


    [Authorize]
    [HttpDelete]
    [Route("/api/[controller]/avatar")]
    public IActionResult RemoveAvatar()
    {
        Guid subscriberGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);

        string errorMsg = string.Empty;

        if (SubscriberFactory.RemoveAvatar(_repositoryWrapper, _configuration, subscriberGuid, ref errorMsg))
            return Ok(new BasicResponseDto() { StatusCode = 200, Description = "Avatar removed." });
        else
            return Ok(new BasicResponseDto() { StatusCode = 400, Description = errorMsg });
    }

    [HttpPut("/api/[controller]/onboard")]
    public IActionResult Onboard()
    {
        Guid subscriberGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);

        if (subscriberGuid == null || subscriberGuid == Guid.Empty)
            return BadRequest();

        Subscriber subscriber = _db.Subscriber.Where(t => t.IsDeleted == 0 && t.SubscriberGuid == subscriberGuid).FirstOrDefault();

        subscriber.HasOnboarded = 1;
        _db.Subscriber.Update(subscriber);
        _db.SaveChanges();

        return Ok();
    }

    [HttpGet("/api/[controller]/me/partner-web-redirect")]
    public async Task<IActionResult> GetSubscriberPartnerWebRedirectAsync()
    {
        Guid subscriberGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
        if (subscriberGuid == null || subscriberGuid == Guid.Empty)
            return BadRequest();

        Subscriber subscriber = await _db
            .Subscriber.Where(t => t.IsDeleted == 0 && t.SubscriberGuid == subscriberGuid)
            .FirstOrDefaultAsync();

        if (subscriber == null)
            return BadRequest();

        var result = await _db.SubscriberSignUpPartnerReferences.Where(s => s.SubscriberId == subscriber.SubscriberId).FirstOrDefaultAsync();

        if (result.PartnerId == null)
            return Ok(new RedirectDto() { RelativePath = null });

        var redirect = await _db.PartnerWebRedirect.Where(e => e.PartnerId == result.PartnerId).FirstOrDefaultAsync();
        return Ok(new RedirectDto() { RelativePath = redirect?.RelativePath });
    }

    [HttpGet("/api/[controller]/search")]
    [Authorize(Policy = "IsRecruiterOrAdmin")]
    public IActionResult Search(string searchFilter = "any", string searchQuery = null, string searchLocationQuery = null, string sortOrder = null)
    {

        int MaxProfilePageSize = int.Parse(_configuration["CloudTalent:MaxProfilePageSize"]);
        ProfileQueryDto profileQueryDto = new ProfileQueryDto()
        {
            Keywords = searchQuery,
            SourcePartner = searchFilter == null || searchFilter.ToLower() == "any" ? string.Empty : searchFilter,
            Location = searchLocationQuery,
            // must be < 100
            PageSize = MaxProfilePageSize

        };
        if (sortOrder != null)
            profileQueryDto.OrderBy = WebUtility.UrlDecode(sortOrder);
        ProfileSearchResultDto result = _cloudTalentService.ProfileSearch(profileQueryDto);

        return Json(result);
    }

    [Authorize]
    [HttpGet("/api/[controller]/sources")]
    public IActionResult GetSubscriberSources()
    {

        return Ok(_db.SubscriberSources.ProjectTo<SubscriberSourceStatisticDto>(_mapper.ConfigurationProvider).ToList());
    }

    // todo: add security to check token to this route
    [HttpGet("/api/[controller]/{subscriberGuid}/skill")]
    public IActionResult GetSkillsBySubscriber(Guid subscriberGuid)
    {
        var subscriberSkills = _db.Subscriber
            .Where(s => s.IsDeleted == 0 && s.SubscriberGuid.Value == subscriberGuid)
            .Join(_db.SubscriberSkill.Where(ss => ss.IsDeleted == 0), s => s.SubscriberId, sk => sk.SubscriberId, (s, sk) => new { sk.SkillId })
            .Join(_db.Skill.Where(s => s.IsDeleted == 0), x => x.SkillId, s => s.SkillId, (x, s) => s)
            .Distinct()
            .OrderBy(s => s.SkillName)
            .ProjectTo<SkillDto>(_mapper.ConfigurationProvider)
            .ToList();

        return Ok(subscriberSkills);
    }

    [Authorize]
    [HttpGet("/api/[controller]/{subscriberGuid}/file/{fileGuid}")]
    public async Task<IActionResult> DownloadFile(Guid subscriberGuid, Guid fileGuid)
    {
        Guid loggedInUserGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
        var isAuth = await _authorizationService.AuthorizeAsync(User, "IsRecruiterPolicy");

        if (loggedInUserGuid != subscriberGuid && !isAuth.Succeeded)
            return Unauthorized();

        Subscriber subscriber = _db.Subscriber.Where(s => s.SubscriberGuid.Equals(subscriberGuid))
            .Include(s => s.SubscriberFile)
            .First();

        SubscriberFile file = subscriber.SubscriberFile.Where(f => f.SubscriberFileGuid.Equals(fileGuid)).First();

        if (file == null)
            return NotFound(new BasicResponseDto { StatusCode = 404, Description = "File not found. " });

        // track the subscriber action if performed by someone other than the user who owns the file
        if (loggedInUserGuid != subscriber.SubscriberGuid.Value)
            new SubscriberActionFactory(_repositoryWrapper, _db, _configuration, _syslog, _cache).TrackSubscriberAction(loggedInUserGuid, "Download resume", "Subscriber", subscriber.SubscriberGuid);

        return File(await _cloudStorage.OpenReadAsync(file.BlobName), "application/octet-stream", Path.GetFileName(file.BlobName));
    }

    [Authorize]
    [HttpDelete("/api/[controller]/{subscriberGuid}/file/{fileGuid}")]
    public async Task<IActionResult> DeleteFile(Guid subscriberGuid, Guid fileGuid)
    {
        Guid userGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
        if (userGuid != subscriberGuid)
            return Unauthorized();

        Subscriber subscriber = _db.Subscriber.Where(s => s.SubscriberGuid.Equals(subscriberGuid))
            .Include(s => s.SubscriberFile)
            .First();
        SubscriberFile file = subscriber.SubscriberFile.Where(f => f.SubscriberFileGuid.Equals(fileGuid)).First();

        if (file == null)
            return NotFound(new BasicResponseDto() { StatusCode = 404, Description = "File not found." });

        if (!await _cloudStorage.DeleteFileAsync(file.BlobName))
            return BadRequest();

        file.IsDeleted = 1;
        _db.SubscriberFile.Update(file);
        await _db.SaveChangesAsync();

        return Ok();
    }

    [Authorize]
    [HttpPost("/api/[controller]/me/job-favorite/map")]
    public async Task<IActionResult> JobFavoriteMap([FromBody] List<Guid> jobGuids)
    {
        Guid subscriberGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
        var map = await _subscriberService.GetSubscriberJobPostingFavoritesByJobGuid(subscriberGuid, jobGuids);
        return Ok(map);
    }

    [HttpGet("/api/[controller]/me/job-alerts")]
    public async Task<IActionResult> GetSubscriberJobAlerts(int? page, int? timeZoneOffset)
    {
        Guid userGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
        var jobAlerts = await _repositoryWrapper.JobPostingAlertRepository.GetAllJobPostingAlertsBySubscriber(userGuid);

        if (timeZoneOffset.HasValue)
        {
            foreach (var jobAlert in jobAlerts)
            {
                // construct a datetime object that represents the next utc execution time for the job alert
                DateTime utcExecutionDate = new DateTime(
                    DateTime.UtcNow.Year,
                    DateTime.UtcNow.Month,
                    jobAlert.Frequency == Frequency.Weekly ? DateTime.UtcNow.Next(jobAlert.ExecutionDayOfWeek.Value).Day : DateTime.UtcNow.Day,
                    jobAlert.ExecutionHour,
                    jobAlert.ExecutionMinute,
                    0);
                // adjust day, hour, and minute based on time zone offset for local time
                var timespanTimeZoneOffset = new TimeSpan(0, timeZoneOffset.Value, 0);
                // calculate the local execution date
                DateTime localExecutionDate = utcExecutionDate.Subtract(timespanTimeZoneOffset);
                // update the job alert properties
                jobAlert.ExecutionDayOfWeek = localExecutionDate.DayOfWeek;
                jobAlert.ExecutionHour = localExecutionDate.Hour;
                jobAlert.ExecutionMinute = localExecutionDate.Minute;
            }
        }

        var query = jobAlerts.Select(ja => new JobPostingAlertDto()
        {
            Description = ja.Description,
            ExecutionDayOfWeek = ja.ExecutionDayOfWeek,
            ExecutionHour = ja.ExecutionHour,
            ExecutionMinute = ja.ExecutionMinute,
            Frequency = ja.Frequency.ToString(),
            JobPostingAlertGuid = ja.JobPostingAlertGuid
        });

        var result = new PagingDto<JobPostingAlertDto>()
        {
            Page = page.HasValue ? page.Value : 1,
            PageSize = 10,
            Count = query.Count()
        };
        result.Results = query.Skip((result.Page - 1) * result.PageSize).Take(result.PageSize).ToList();

        return Ok(result);
    }

    [HttpGet("/api/[controller]/me/jobs")]
    public async Task<IActionResult> GetSubscriberJobFavorites(int? page)
    {

        // todo: move to service
        Guid userGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
        Subscriber subscriber = _db.Subscriber.Where(s => s.SubscriberGuid.Equals(userGuid))
            .First();

        if (subscriber == null)
            return StatusCode(404, false);

        List<JobDto> favorites = await _jobPostingService.GetSubscriberJobFavorites(subscriber.SubscriberId);

        var result = new PagingDto<UpDiddyLib.Dto.User.JobDto>()
        {
            Page = page.HasValue ? page.Value : 1,
            PageSize = 10,
            Count = favorites.Count
        };


        result.Results = await favorites.Skip((result.Page - 1) * result.PageSize).Take(result.PageSize).ToListAsync();

        return Ok(result);
    }

    private void SendGatedDownloadLink(string email, string link)
    {
        _hangfireService.Enqueue(() =>
         _sysEmail.SendTemplatedEmailAsync(
             email,
             _configuration["SysEmail:Transactional:TemplateIds:GatedDownload-LinkEmail"],
             new
             {
                 fileDownloadLinkUrl = link
             },
             Constants.SendGridAccount.Transactional,
             null,
             null,
             null,
             null
         ));
    }

    #region SubscriberNotes
    /// <summary>
    /// Save Notes
    /// </summary>
    /// <param name="subscriberNotes"></param>
    /// <returns></returns>
    [Authorize(Policy = "IsRecruiterPolicy")]
    [HttpPost("/api/[controller]/save-notes")]
    public async Task<IActionResult> SaveSubscriberNotes([FromBody]SubscriberNotesDto subscriberNotes)
    {
        try
        {
            if (ModelState.IsValid)
            {
                //get user logged in who is by default recruiter
                subscriberNotes.RecruiterGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                await _subscriberService.SaveSubscriberNotesAsync(subscriberNotes);
                return Ok(new BasicResponseDto { StatusCode = 200, Description = "Saved Successfully." });
            }
            else
            {
                _syslog.Log(LogLevel.Trace, $"SubscriberController.SaveSubscriberNotes : Invalid Subscriber notes data: {JsonConvert.SerializeObject(subscriberNotes)}");
                return BadRequest(new BasicResponseDto { StatusCode = 400, Description = "Invalid data." });
            }

        }
        catch (Exception ex)
        {
            _syslog.Log(LogLevel.Error, $"SubscriberController.SaveSubscriberNotes : Error occured when saving notes for data: {JsonConvert.SerializeObject(subscriberNotes)} with message={ex.Message}", ex);
            return StatusCode(500, new BasicResponseDto { StatusCode = 400, Description = "Internal Server Error." });
        }
    }

    [HttpGet("/api/[controller]/notes/{subscriberGuid}")]
    [Authorize(Policy = "IsRecruiterPolicy")]
    public async Task<IActionResult> SearchSubscriberNotes(string subscriberGuid, string searchQuery = null)
    {
        //get user logged in who is by default recruiter
        var recruiterGuid = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

        var subscriberNotesList = await _subscriberService.GetSubscriberNotesBySubscriberGuid(subscriberGuid, recruiterGuid, searchQuery);
        return Ok(subscriberNotesList);
    }

    [Authorize(Policy = "IsRecruiterPolicy")]
    [HttpDelete("/api/[controller]/notes/{subscriberNotesGuid}")]
    public async Task<IActionResult> DeleteSubscriberNote(Guid subscriberNotesGuid)
    {
        var isDeleted = await _subscriberService.DeleteSubscriberNote(subscriberNotesGuid);

        return Ok(isDeleted);
    }
    #endregion

    [Authorize]
    [HttpDelete("/api/[controller]/delete-notification/{notificationGuid}")]
    public async Task<IActionResult> DeleteSubscriberNotification(Guid notificationGuid)
    {
        if (notificationGuid == null || notificationGuid == Guid.Empty)
            return BadRequest();

        Guid loggedInUserGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
        bool isSuccess = await _subscriberNotificationService.DeleteSubscriberNotification(loggedInUserGuid, notificationGuid);

        if (!isSuccess)
            return StatusCode(500, false);
        else
            return Ok(new BasicResponseDto { StatusCode = 200, Description = "Subscriber notification deleted successfully." });
    }

    [AllowAnonymous]
    [HttpPut("/api/[controller]/{subscriberGuid}/toggle-notification-emails/{isEnabled}")]
    public async Task<IActionResult> ToggleSubscriberNotificationEmail(Guid subscriberGuid, string isEnabled)
    {
        bool _isEnabled = false;
        if (!bool.TryParse(isEnabled, out _isEnabled))
            return BadRequest();

        bool isSuccess = await _subscriberService.ToggleSubscriberNotificationEmail(subscriberGuid, _isEnabled);

        if (!isSuccess)
            return Ok(new BasicResponseDto { StatusCode = 404, Description = "Subscriber notification email setting was not updated successfully." });
        else
            return Ok(new BasicResponseDto { StatusCode = 200, Description = "Subscriber notification email setting updated successfully." });
    }



    [AllowAnonymous]
    [HttpPost("/api/[controller]/existing-user-sign-up")]
    public async Task<IActionResult> ExistingUserSignUp([FromBody] CreateUserDto createUserDto)
    {
        bool isExistingUserSignupSuccessful = await _subscriberService.ExistingSubscriberSignUp(createUserDto);

        if (isExistingUserSignupSuccessful)
            return Ok(new BasicResponseDto() { StatusCode = 200, Description = "Subscriber has been signed up successfully." });
        else
            return Ok(new BasicResponseDto() { StatusCode = 400, Description = "An error occurred processing the existing user sign up." });
    }

    [HttpPut("read-notification")]
    public async Task<IActionResult> SubscriberReadNotification([FromBody] NotificationDto ReadNotification)
    {
        if (ReadNotification == null || ReadNotification.NotificationGuid == null)
            return BadRequest();

        Guid loggedInUserGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
        SubscriberDto subscriberDto = await SubscriberFactory.GetSubscriber(_repositoryWrapper, loggedInUserGuid, _syslog, _mapper);
        Notification ExistingNotification = _repositoryWrapper.NotificationRepository.GetByConditionAsync(n => n.NotificationGuid == ReadNotification.NotificationGuid).Result.FirstOrDefault();

        if (loggedInUserGuid == subscriberDto.SubscriberGuid)
        {
            var t = await _repositoryWrapper.SubscriberNotificationRepository.GetByConditionWithTrackingAsync(
                n => n.NotificationId == ExistingNotification.NotificationId &&
                n.SubscriberId == subscriberDto.SubscriberId);

            SubscriberNotification SubscriberNotificationEntry = t.FirstOrDefault();

            if (SubscriberNotificationEntry == null)
                return NotFound();


            SubscriberNotificationEntry.HasRead = 1;
            SubscriberNotificationEntry.ModifyDate = DateTime.UtcNow;

            _repositoryWrapper.SubscriberNotificationRepository.Update(SubscriberNotificationEntry);
            await _repositoryWrapper.SubscriberNotificationRepository.SaveAsync();

            return Ok(new BasicResponseDto { StatusCode = 200, Description = "SubscriberNotification " + SubscriberNotificationEntry.SubscriberNotificationGuid + " successfully updated." });
        }
        else
        {
            return Unauthorized();
        }
    }

    [HttpGet]
    [Route("subscriber-details")]
    public async Task<IActionResult> GetSubscriber(ODataQueryOptions<Subscriber> options)
    {
        try
        {
            var subscriber = await _subscriberService.GetSubscriber(options);
            return Ok(_mapper.Map<SubscriberDto>(subscriber));
        }
        catch (Exception ex)
        {
            _syslog.Log(LogLevel.Error, $"SubscriberController.GetSubscriber : Error occured when retrieving recruiter with message={ex.Message}", ex);
            return StatusCode(500);
        }
    }

    [HttpGet]
    [Route("/api/[controller]/failed-subscribers")]
    public async Task<IActionResult> GetFailedSubscribersSummaryAsync()
    {
        try
        {
            var subscriber = await _subscriberService.GetFailedSubscribersSummaryAsync();
            return Ok(_mapper.Map<List<FailedSubscriberDto>>(subscriber));
        }
        catch (Exception ex)
        {
            _syslog.Log(LogLevel.Error, $"SubscriberController.GetFailedSubscribersSummaryAsync : Error occured when retrieving recruiter with message={ex.Message}", ex);
            return StatusCode(500);
        }
    }
}
