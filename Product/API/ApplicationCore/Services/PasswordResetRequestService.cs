using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
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

        public PasswordResetRequestService(IServiceProvider services)
        {
            _repositoryWrapper = services.GetService<IRepositoryWrapper>();
            _mapper = services.GetService<IMapper>();
            _configuration = services.GetService<IConfiguration>();
            _hangfireService = services.GetService<IHangfireService>();
            _sysEmail = services.GetService<ISysEmail>();
        }

        public async Task<Guid> CreatePasswordResetRequest(Guid subscriberGuid)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> ConsumePasswordResetRequest(Guid passwordResetRequestGuid)
        {
            throw new NotImplementedException();
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
