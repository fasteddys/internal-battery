using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UpDiddyApi.ApplicationCore.Exceptions;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.ApplicationCore.Services.Identity.Interfaces;
using UpDiddyApi.Models;
using UpDiddyLib.Dto.User;
using UpDiddyLib.Helpers;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class PasswordResetRequestService : IPasswordResetRequestService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IHangfireService _hangfireService;
        private readonly ISysEmail _sysEmail;
        private readonly IUserService _userService;

        public PasswordResetRequestService(IServiceProvider services)
        {
            _repositoryWrapper = services.GetService<IRepositoryWrapper>();
            _mapper = services.GetService<IMapper>();
            _configuration = services.GetService<IConfiguration>();
            _hangfireService = services.GetService<IHangfireService>();
            _sysEmail = services.GetService<ISysEmail>();
            _userService = services.GetService<IUserService>();
        }

        public async Task CreatePasswordResetRequest(Guid subscriberGuid)
        {
            // todo: we do not have a repository for this. it is hard-coded everywhere else, cannot afford to take the time now to refactor it
            int inProcessRedemptionStatusId = 1;

            int passwordResetExpirationInHours = 0;
            if (!int.TryParse(_configuration["PasswordResetExpirationInHours"], out passwordResetExpirationInHours))
                throw new NotFoundException("PasswordResetExpirationInHours not found");

            var subscriber = await _repositoryWrapper.SubscriberRepository.GetByGuid(subscriberGuid);
            if (subscriber == null)
                throw new NotFoundException("Subscriber not found");

            Guid newPasswordResetRequestGuid = Guid.NewGuid();

            _repositoryWrapper.PasswordResetRequestRepository.Create(
                new PasswordResetRequest()
                {
                    CreateDate = DateTime.UtcNow,
                    CreateGuid = Guid.Empty,
                    ExpirationDate = DateTime.UtcNow.AddHours(passwordResetExpirationInHours),
                    IsDeleted = 0,
                    PasswordResetRequestGuid = newPasswordResetRequestGuid,
                    RedemptionStatusId = inProcessRedemptionStatusId,
                    ResetDate = null,
                    SubscriberId = subscriber.SubscriberId
                });
            await _repositoryWrapper.PasswordResetRequestRepository.SaveAsync();

            this.SendPasswordResetEmail(subscriber.Email, passwordResetExpirationInHours, newPasswordResetRequestGuid);
        }

        public async Task<bool> ConsumePasswordResetRequest(ResetPasswordDto resetPasswordDto)
        {
            // todo: we do not have a repository for this. it is hard-coded everywhere else, cannot afford to take the time now to refactor it
            int completedRedemptionStatusId = 2;

            var passwordResetRequest = await _repositoryWrapper.PasswordResetRequestRepository.GetByGuid(resetPasswordDto.PasswordResetRequestGuid);

            if (passwordResetRequest == null)
                return false;

            if (passwordResetRequest.ExpirationDate <= DateTime.UtcNow)
                return false;


            var subscriber = await _repositoryWrapper.SubscriberRepository.GetSubscriberByIdAsync(passwordResetRequest.SubscriberId);

            if (string.IsNullOrWhiteSpace(subscriber.Auth0UserId))
                return false;

            var isPasswordResetSuccessfully = await _userService.ResetPasswordAsync(subscriber.Auth0UserId, resetPasswordDto.Password);

            if (isPasswordResetSuccessfully)
            {

                passwordResetRequest.ModifyDate = DateTime.UtcNow;
                passwordResetRequest.ModifyGuid = Guid.Empty;
                passwordResetRequest.RedemptionStatusId = completedRedemptionStatusId;
                passwordResetRequest.ResetDate = DateTime.UtcNow;
                _repositoryWrapper.PasswordResetRequestRepository.Update(passwordResetRequest);
                await _repositoryWrapper.PasswordResetRequestRepository.SaveAsync();

                return true;
            }
            else
                return false;
        }

        private void SendPasswordResetEmail(string email, int expirationInHours, Guid passwordResetRequestGuid)
        {
            _hangfireService.Enqueue(() =>
                _sysEmail.SendTemplatedEmailAsync(
                    email,
                    _configuration["SysEmail:Transactional:TemplateIds:PasswordResetRequest-LinkEmail"],
                    new
                    {
                        expirationInHours = 8,
                        verificationLink = _configuration["Environment:BaseUrl"] + "session/verify/" + passwordResetRequestGuid.ToString()
                    },
                    Constants.SendGridAccount.Transactional,
                    null,
                    null,
                    null,
                    null)
                );
        }
    }
}
