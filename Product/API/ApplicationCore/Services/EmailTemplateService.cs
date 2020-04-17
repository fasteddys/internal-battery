using AutoMapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyLib.Domain.Models;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class EmailTemplateService : IEmailTemplateService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;
        public EmailTemplateService(IRepositoryWrapper repositoryWrapper, IMapper mapper, IConfiguration config)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _config = config;
        }

        public async Task<EmailTemplateListDto> GetEmailTemplates(int limit, int offset, string sort, string order)
        {

            List<EmailTemplateDto> templates = await _repositoryWrapper.EmailTemplateRepository.GetEmailTemplates(limit, offset, sort, order);
            return _mapper.Map<EmailTemplateListDto>(templates);

        }

        public async Task<IEnumerable<HydratedEmailBody>> HydrateEmailTemplate(string template, Guid recruiterGuid, IEnumerable<Guid> profileGuids)
        {
            // STUB!
            await Task.Yield();
            return Enumerable.Empty<HydratedEmailBody>();
        }
    }
}
