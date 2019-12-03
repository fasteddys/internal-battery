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

        public async Task<bool> CheckValidityOfPasswordResetRequest(Guid passwordResetRequestGuid)
        {
            var isPasswordResetRequestValidResult = await ValidatePasswordResetRequest(passwordResetRequestGuid);
            return isPasswordResetRequestValidResult.Item2;
        }

        public async Task<bool> ConsumePasswordResetRequest(ResetPasswordDto resetPasswordDto)
        {
            // todo: we do not have a repository for this. it is hard-coded everywhere else, cannot afford to take the time now to refactor it
            int completedRedemptionStatusId = 2;

            var passwordResetRequest = await ValidatePasswordResetRequest(resetPasswordDto.PasswordResetRequestGuid);

            if (passwordResetRequest.Item2)
            {
                // password reset request is valid - continue processing the request
                var isPasswordResetSuccessfully = await _userService.ResetPasswordAsync(passwordResetRequest.Item1.Subscriber.Auth0UserId, resetPasswordDto.Password);

                // only update the password reset request record if we changed both passwords successfully
                if (isPasswordResetSuccessfully)
                {
                    passwordResetRequest.Item1.ModifyDate = DateTime.UtcNow;
                    passwordResetRequest.Item1.ModifyGuid = Guid.Empty;
                    passwordResetRequest.Item1.RedemptionStatusId = completedRedemptionStatusId;
                    passwordResetRequest.Item1.ResetDate = DateTime.UtcNow;
                    _repositoryWrapper.PasswordResetRequestRepository.Update(passwordResetRequest.Item1);
                    await _repositoryWrapper.PasswordResetRequestRepository.SaveAsync();

                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                // password reset request is invalid
                return false;
            }
        }

        /// <summary>
        /// Shared method that checks to see if there is an active (unused) password reset request which has not expired
        /// and that the associated subscriber has been created in Auth0 (we cannot change a password for a user that 
        /// has not yet been migrated).
        /// </summary>
        /// <param name="passwordResetRequestGuid"></param>
        /// <returns></returns>
        private async Task<Tuple<PasswordResetRequest, bool>> ValidatePasswordResetRequest(Guid passwordResetRequestGuid)
        {
            if (passwordResetRequestGuid == null || passwordResetRequestGuid == Guid.Empty)
                return new Tuple<PasswordResetRequest, bool>(null, false);

            var passwordResetRequest = await _repositoryWrapper.PasswordResetRequestRepository.GetByGuid(passwordResetRequestGuid);

            if (passwordResetRequest == null)
                return new Tuple<PasswordResetRequest, bool>(null, false);

            if (passwordResetRequest.ExpirationDate <= DateTime.UtcNow || passwordResetRequest.ResetDate.HasValue || passwordResetRequest.RedemptionStatusId == 2)
                return new Tuple<PasswordResetRequest, bool>(passwordResetRequest, false);

            var subscriber = await _repositoryWrapper.SubscriberRepository.GetSubscriberByIdAsync(passwordResetRequest.SubscriberId);

            if (string.IsNullOrWhiteSpace(subscriber.Auth0UserId))
                return new Tuple<PasswordResetRequest, bool>(passwordResetRequest, false);

            return new Tuple<PasswordResetRequest, bool>(passwordResetRequest, true);
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
                        verificationLink = _configuration["Environment:BaseUrl"] + "session/changepassword/" + passwordResetRequestGuid.ToString()
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
