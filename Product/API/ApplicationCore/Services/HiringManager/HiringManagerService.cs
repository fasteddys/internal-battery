 
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




        
        public async Task<bool> AddHiringManager(Guid subscriberGuid, bool nonBlocking = true)
        {
            _logger.LogInformation($"HiringManagerService:AddHiringManager  Starting for subscriber {subscriberGuid} ");
            // validate the subscriber is valid
            var subscriber = await _repositoryWrapper.SubscriberRepository.GetSubscriberByGuidAsync(subscriberGuid);
            if (subscriber == null)
                throw new FailedValidationException($"HiringManagerService:AddHiringManager Cannot locate subscriber {subscriberGuid}");

            // TODO Vivek
            // validate the subscriber is not already a hiring manager
            // if already hiring manager 
            //     throw new FailedValidationException($"HiringManagerService:AddHiringManager {subscriberGuid} is already a hiring manger");


            try
            {
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
