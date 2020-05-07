 
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Exceptions;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Interfaces.Business.HiringManager;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.ApplicationCore.Services.Identity;
using UpDiddyApi.ApplicationCore.Services.Identity.Interfaces;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Services.HiringManager
{
    public class HiringManagerService : IHiringManagerService
    {

        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;
        private readonly ILogger _logger;
        private readonly IHangfireService _hangfireService;


        public HiringManagerService(IConfiguration configuration, IRepositoryWrapper repositoryWrapper, IMapper mapper, IUserService userService, ILogger<HiringManagerService> logger, IHangfireService hangfireService)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _configuration = configuration;
            _userService = userService;
            _logger = logger;
            _hangfireService = hangfireService;
        }

        public async Task<HiringManagerDto> GetHiringManagerBySubscriberGuid(Guid subscriberGuid)
        {
            if (subscriberGuid == Guid.Empty)
                throw new FailedValidationException($"HiringManagerService:GetHiringManagerBySubscriberGuid subscriber guid cannot be empty({subscriberGuid})");


            // validate the subscriber is valid
            var subscriber = await _repositoryWrapper.SubscriberRepository.GetSubscriberByGuidAsync(subscriberGuid);

            if (subscriber == null)
                throw new FailedValidationException($"HiringManagerService:GetHiringManagerBySubscriberGuid Cannot locate subscriber {subscriberGuid}");

            HiringManagerDto hiringManagerDto = null;
            try
            {
                var hiringManagerEntity = await _repositoryWrapper.HiringManagerRepository.GetHiringManagerBySubscriberId(subscriber.SubscriberId); 

                if(hiringManagerEntity == null)
                {
                    throw new FailedValidationException($"HiringManagerService:GetHiringManagerBySubscriberGuid Cannot locate hiring manager for subscriber: {subscriberGuid}");
                }

                //map hiring manage entity to dto
                hiringManagerDto = _mapper.Map<HiringManagerDto>(hiringManagerEntity);
            }
            catch (Exception ex)
            {
                _logger.LogError($"HiringManagerService:GetHiringManagerBySubscriberGuid  Error: {ex.ToString()} ");
            }

            return hiringManagerDto;
        }


        public async Task UpdateHiringManager(Guid subscriberGuid, HiringManagerDto hiringManager) 
        {
            _logger.LogInformation($"HiringManagerService:UpdateHiringManager  Starting for subscriber {subscriberGuid} ");
            if(subscriberGuid == Guid.Empty)
                throw new FailedValidationException($"HiringManagerService:UpdateHiringManager subscriber guid cannot be empty({subscriberGuid})");

            if (hiringManager == null)
                throw new FailedValidationException($"HiringManagerService:UpdateHiringManager HiringManagerDto cannot be null");

            // validate the subscriber is valid
            var subscriber = await _repositoryWrapper.SubscriberRepository.GetSubscriberByGuidAsync(subscriberGuid);

            if (subscriber == null)
                throw new FailedValidationException($"HiringManagerService:UpdateHiringManager Cannot locate subscriber {subscriberGuid}");

            var hiringManagerEntity = await _repositoryWrapper.HiringManagerRepository.GetHiringManagerBySubscriberId(subscriber.SubscriberId);

            if (hiringManagerEntity != null) throw new FailedValidationException($"HiringManagerService:UpdateHiringManager {subscriberGuid} is not a hiring manager");

            //update the subscriber and company record for the HM in DB
            try
            {
                await _repositoryWrapper.HiringManagerRepository.UpdateHiringManager(subscriber.SubscriberId, hiringManager);
            }
            catch (Exception ex)
            {
                _logger.LogError($"HiringManagerService:UpdateHiringManager  Error: {ex.ToString()} ");
            }

            _logger.LogInformation($"HiringManagerService:UpdateHiringManager  Done for Hiring Manager with subscriber: {subscriberGuid} ");

        }


        public async Task<bool> AddHiringManager(Guid subscriberGuid, bool nonBlocking = true)
        {
            _logger.LogInformation($"HiringManagerService:AddHiringManager  Starting for subscriber {subscriberGuid} ");
            // validate the subscriber is valid
            var subscriber = await _repositoryWrapper.SubscriberRepository.GetSubscriberByGuidAsync(subscriberGuid);
            if (subscriber == null)
                throw new FailedValidationException($"HiringManagerService:AddHiringManager Cannot locate subscriber {subscriberGuid}");

            try
            {
                //check if hiring manager for subscriberid exists
                var hiringManager = await _repositoryWrapper.HiringManagerRepository.GetHiringManagerBySubscriberId(subscriber.SubscriberId);

                if (hiringManager == null)
                    await _repositoryWrapper.HiringManagerRepository.AddHiringManager(subscriber.SubscriberId);

                if (nonBlocking)
                {
                    _logger.LogInformation($"HiringManagerService:AddHiringManager : Background job starting for subscriber {subscriberGuid}");
                    _hangfireService.Enqueue<HiringManagerService>(j => j._AddHiringManager(subscriber));
                    return true;
                }
                else
                {
                    _logger.LogInformation($"HiringManagerService:AddHiringManager : awaiting _AddHiringManager for subscriber {subscriberGuid}");
                    await _AddHiringManager(subscriber);
                }            
            }
            catch (Exception ex )
            {
                _logger.LogError($"HiringManagerService:AddHiringManager  Error: {ex.ToString()} ");
            }
            _logger.LogInformation($"HiringManagerService:AddHiringManager  Done for subscriber {subscriberGuid} ");

            return true;
        }


        public  async Task<bool> _AddHiringManager(Subscriber subscriber)
        {
            _logger.LogInformation($"HiringManagerService:_AddHiringManager  Starting for subscriber {subscriber.SubscriberGuid} ");
            var getUserResponse = await _userService.GetUserByEmailAsync(subscriber.Email);
            if (!getUserResponse.Success || string.IsNullOrWhiteSpace(getUserResponse.User.UserId))
                throw new ApplicationException("User could not be found in Auth0");
            _logger.LogInformation($"HiringManagerService:_AddHiringManager  Calling user service for  {getUserResponse.User.UserId} ");
            _userService.AssignRoleToUserAsync(getUserResponse.User.UserId, Role.HiringManager);
            _logger.LogInformation($"HiringManagerService:_AddHiringManager  Done");

            return true;
        }







    }
}
