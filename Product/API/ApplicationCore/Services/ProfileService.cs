
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Factory;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.Models;
using UpDiddyApi.Workflow;
using AutoMapper;
using UpDiddyApi.ApplicationCore.Exceptions;
using UpDiddyLib.Domain.Models;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class ProfileService : IProfileService
    {
        private UpDiddyDbContext _db { get; set; }
        private IConfiguration _configuration { get; set; }
        private ICloudStorage _cloudStorage { get; set; }
        private ILogger _logger { get; set; }
        private IRepositoryWrapper _repository { get; set; }
        private readonly IMapper _mapper;
        private ITaggingService _taggingService { get; set; }
        private IHangfireService _hangfireService { get; set; }
        private ISubscriberService _subscriberService { get; set; }

        public ProfileService(UpDiddyDbContext context,
        IConfiguration configuration,
        ICloudStorage cloudStorage,
        IRepositoryWrapper repository,
        ILogger<SubscriberService> logger,
        IMapper mapper,
        ITaggingService taggingService,
        IHangfireService hangfireService,
        ISubscriberService subscriberService)
        {
            _db = context;
            _configuration = configuration;
            _cloudStorage = cloudStorage;
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
            _taggingService = taggingService;
            _hangfireService = hangfireService;
            _subscriberService = subscriberService;
        }

        #region basic profile 

        public async Task<SubscribeProfileBasicDto> GetSubscriberProfileBasicAsync(Guid subscriberGuid)
        {

            SubscribeProfileBasicDto rVal = null;
            try
            {
                var Subscriber = await _subscriberService.GetSubscriberByGuid(subscriberGuid);
                if (Subscriber == null)
                    throw new NotFoundException($"SubscriberGuid {subscriberGuid} does not exist exist");
                // Include state information 
                State state = await _repository.State.GetStateBySubscriberGuid(Subscriber.SubscriberGuid.Value);
                Subscriber.State = state;

                rVal = _mapper.Map<SubscribeProfileBasicDto>(Subscriber);
                // Do not return the auth0UserId 
                rVal.Auth0UserId = string.Empty;
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"SubscriberService.UpdateSubscriberProfileBasicAsync: An error occured while attempting to create a subscriber. Message: {e.Message}", e);
                throw e;
            }
            return rVal;
        }

        public async Task<bool> UpdateSubscriberProfileBasicAsync(SubscribeProfileBasicDto subscribeProfileBasicDto, Guid subscriberGuid)
        {
            try
            {
                var Subscriber = await _subscriberService.GetSubscriberByGuid(subscriberGuid);
                if (Subscriber == null)
                    throw new NotFoundException($"SubscriberGuid {subscriberGuid} does not exist exist");



                int? StateId = null;
                if (string.IsNullOrWhiteSpace(subscribeProfileBasicDto.ProvinceCode) == false)
                {
                    State state = await StateFactory.GetStateByStateCode(_repository, subscribeProfileBasicDto.ProvinceCode);
                    if (state != null)
                        StateId = state.StateId;
                }

                // update the user in the CareerCircle database
                // Note: Do Not allow updates to auth0UserId
                Subscriber.FirstName = !string.IsNullOrWhiteSpace(subscribeProfileBasicDto.FirstName) ? subscribeProfileBasicDto.FirstName : null;
                Subscriber.LastName = !string.IsNullOrWhiteSpace(subscribeProfileBasicDto.LastName) ? subscribeProfileBasicDto.LastName : null;
                Subscriber.PhoneNumber = !string.IsNullOrWhiteSpace(subscribeProfileBasicDto.PhoneNumber) ? subscribeProfileBasicDto.PhoneNumber : null;
                Subscriber.Address = !string.IsNullOrWhiteSpace(subscribeProfileBasicDto.Address) ? subscribeProfileBasicDto.Address : null;
                Subscriber.City = !string.IsNullOrWhiteSpace(subscribeProfileBasicDto.City) ? subscribeProfileBasicDto.City : null;
                Subscriber.PostalCode = !string.IsNullOrWhiteSpace(subscribeProfileBasicDto.PostalCode) ? subscribeProfileBasicDto.PostalCode : null;
                Subscriber.Biography = !string.IsNullOrWhiteSpace(subscribeProfileBasicDto.Biography) ? subscribeProfileBasicDto.Biography : null;
                Subscriber.Title = !string.IsNullOrWhiteSpace(subscribeProfileBasicDto.Title) ? subscribeProfileBasicDto.Title : null;
                Subscriber.ModifyDate = DateTime.UtcNow;
                Subscriber.StateId = StateId;

                await _repository.SubscriberRepository.SaveAsync();

                // add the user to the Google Talent Cloud
                _hangfireService.Enqueue<ScheduledJobs>(j => j.CloudTalentAddOrUpdateProfile(subscriberGuid));

            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"SubscriberService.UpdateSubscriberProfileBasicAsync: An error occured while attempting to create a subscriber. Message: {e.Message}", e);
                throw e;
            }
            return true;
        }


        public async Task<bool> CreateNewSubscriberAsync(SubscribeProfileBasicDto subscribeProfileBasicDto)
        {
            bool isSubscriberCreatedSuccessfully = false;

            try
            {

                var Subscriber = await _subscriberService.GetSubscriberByGuid(subscribeProfileBasicDto.SubscriberGuid);
                if (Subscriber != null)
                    throw new AlreadyExistsException($"SubscriberGuid {subscribeProfileBasicDto.SubscriberGuid} already exists");

                Subscriber = await _subscriberService.GetSubscriberByEmail(subscribeProfileBasicDto.Email);
                if (Subscriber != null)
                    throw new AlreadyExistsException($"A subscriber already exists with {subscribeProfileBasicDto.Email} as their email");



                int? StateId = null;
                if (string.IsNullOrWhiteSpace(subscribeProfileBasicDto.ProvinceCode) == false)
                {
                    State state = await StateFactory.GetStateByStateCode(_repository, subscribeProfileBasicDto.ProvinceCode);
                    if (state != null)
                        StateId = state.StateId;
                }

                // create the user in the CareerCircle database
                await _repository.SubscriberRepository.Create(new Subscriber()
                {
                    //TODO: Need to revisit the guid creation later to make sure it's in line with the auth0 process
                    SubscriberGuid = Guid.NewGuid(),
                    Auth0UserId = subscribeProfileBasicDto.Auth0UserId,
                    Email = subscribeProfileBasicDto.Email,
                    FirstName = !string.IsNullOrWhiteSpace(subscribeProfileBasicDto.FirstName) ? subscribeProfileBasicDto.FirstName : null,
                    LastName = !string.IsNullOrWhiteSpace(subscribeProfileBasicDto.LastName) ? subscribeProfileBasicDto.LastName : null,
                    PhoneNumber = !string.IsNullOrWhiteSpace(subscribeProfileBasicDto.PhoneNumber) ? subscribeProfileBasicDto.PhoneNumber : null,
                    Address = !string.IsNullOrWhiteSpace(subscribeProfileBasicDto.Address) ? subscribeProfileBasicDto.Address : null,
                    City = !string.IsNullOrWhiteSpace(subscribeProfileBasicDto.City) ? subscribeProfileBasicDto.City : null,
                    PostalCode = !string.IsNullOrWhiteSpace(subscribeProfileBasicDto.PostalCode) ? subscribeProfileBasicDto.PostalCode : null,
                    CreateDate = DateTime.UtcNow,
                    CreateGuid = Guid.Empty,
                    StateId = StateId,
                    IsDeleted = 0
                });



                await _repository.SubscriberRepository.SaveAsync();
                var subscriber = _repository.SubscriberRepository.GetSubscriberByGuid(subscribeProfileBasicDto.SubscriberGuid);

                // add the user to the Google Talent Cloud
                _hangfireService.Enqueue<ScheduledJobs>(j => j.CloudTalentAddOrUpdateProfile(subscribeProfileBasicDto.SubscriberGuid));


                isSubscriberCreatedSuccessfully = true;
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"SubscriberService.CreateNewSubscriberAsync: An error occured while attempting to create a subscriber. Message: {e.Message}", e);
                throw (e);
            }

            return isSubscriberCreatedSuccessfully;
        }

        #endregion

        #region social profile

        public async Task<SubscriberProfileSocialDto> GetSubscriberProfileSocialAsync(Guid subscriberGuid)
        {
            SubscriberProfileSocialDto rVal = null;
            try
            {
                var Subscriber = await _subscriberService.GetSubscriberByGuid(subscriberGuid);
                if (Subscriber == null)
                    throw new NotFoundException($"SubscriberGuid {subscriberGuid} does not exist exist");

                rVal = _mapper.Map<SubscriberProfileSocialDto>(Subscriber);
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"SubscriberService.UpdateSubscriberProfileBasicAsync: An error occured while attempting to create a subscriber. Message: {e.Message}", e);
                throw e;
            }
            return rVal;
        }


        public async Task<bool> UpdateSubscriberProfileSocialAsync(SubscriberProfileSocialDto subscribeProfileBasicDto, Guid subscriberGuid)
        {
            try
            {
                var Subscriber = await _subscriberService.GetSubscriberByGuid(subscriberGuid);
                if (Subscriber == null)
                    throw new NotFoundException($"SubscriberGuid {subscriberGuid} does not exist exist");

                if (Subscriber.SubscriberGuid != subscriberGuid)
                    throw new InvalidOperationException($"Not owner of profile");

                // update the user in the CareerCircle database 
                Subscriber.FacebookUrl = !string.IsNullOrWhiteSpace(subscribeProfileBasicDto.FacebookUrl) ? subscribeProfileBasicDto.FacebookUrl : null;
                Subscriber.LinkedInUrl = !string.IsNullOrWhiteSpace(subscribeProfileBasicDto.LinkedInUrl) ? subscribeProfileBasicDto.LinkedInUrl : null;
                Subscriber.GithubUrl = !string.IsNullOrWhiteSpace(subscribeProfileBasicDto.GithubUrl) ? subscribeProfileBasicDto.GithubUrl : null;
                Subscriber.StackOverflowUrl = !string.IsNullOrWhiteSpace(subscribeProfileBasicDto.StackOverflowUrl) ? subscribeProfileBasicDto.StackOverflowUrl : null;
                Subscriber.TwitterUrl = !string.IsNullOrWhiteSpace(subscribeProfileBasicDto.TwitterUrl) ? subscribeProfileBasicDto.TwitterUrl : null;
                Subscriber.LinkedInAvatarUrl = !string.IsNullOrWhiteSpace(subscribeProfileBasicDto.LinkedInAvatarUrl) ? subscribeProfileBasicDto.LinkedInAvatarUrl : null;
                if (subscribeProfileBasicDto.LinkedInSyncDate != null)
                    Subscriber.LinkedInSyncDate = subscribeProfileBasicDto.LinkedInSyncDate;

                await _repository.SubscriberRepository.SaveAsync();
                // add the user to the Google Talent Cloud
                _hangfireService.Enqueue<ScheduledJobs>(j => j.CloudTalentAddOrUpdateProfile(subscribeProfileBasicDto.SubscriberGuid));

            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"SubscriberService.UpdateSubscriberProfileBasicAsync: An error occured while attempting to create a subscriber. Message: {e.Message}", e);
                throw e;
            }
            return true;
        }

        #endregion
    }

}
