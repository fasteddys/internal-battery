 
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Exceptions;
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


        public HiringManagerService(IConfiguration configuration, IRepositoryWrapper repositoryWrapper, IMapper mapper, IUserService userService, ILogger<HiringManagerService> logger)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _configuration = configuration;
            _userService = userService;
            _logger = logger;
        }




        // TODO JAB Move to hiring manager service 
        public async Task<bool> AddHiringManager(Guid subscriberGuid)
        {

            _logger.LogInformation($"HiringManagerService:AdHiringManager  Starting for subscriber {subscriberGuid} ");
            try
            {
                // validate that the subscriber is real
                var subscriber = await _repositoryWrapper.SubscriberRepository.GetSubscriberByGuidAsync(subscriberGuid);
                if ( subscriber == null )
                    throw new FailedValidationException($"HiringManagerService:AdHiringManager Cannot locate subscriber {subscriberGuid}")



                // todo jab validate that the subscriber is not already a hiring manager 

                // todo call Auth0 admin endpoint to add role to subscriber 
                await AssignRecruiterPermissionsAsync(subscriber);

            }
            catch (Exception ex )
            {
                _logger.LogError($"HiringManagerService:AdHiringManager  Error: {ex.ToString()} ");
            }

            _logger.LogInformation($"HiringManagerService:AdHiringManager  Done for subscriber {subscriberGuid} ");

            return true;
        }



        private async Task AssignRecruiterPermissionsAsync(Subscriber subscriber)
        { 
            var getUserResponse = await _userService.GetUserByEmailAsync(subscriber.Email);
            if (!getUserResponse.Success || string.IsNullOrWhiteSpace(getUserResponse.User.UserId))
                throw new ApplicationException("User could not be found in Auth0");
            _userService.AssignRoleToUserAsync(getUserResponse.User.UserId, Role.HiringManager);
        }




    }
}
