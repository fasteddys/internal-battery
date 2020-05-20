using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business.B2B;
using UpDiddyLib.Domain.Models.B2B;
using UpDiddyLib.Dto;
using UpDiddyLib.Helpers;

namespace UpDiddyApi.ApplicationCore.Services.B2B.CareerTalentPipeline
{
    public class CareerTalentPipelineService : ICareerTalentPipelineService
    {
        private readonly ISysEmail _emailService;
        private readonly CareerTalentPipelineOptions _options;

        public CareerTalentPipelineService(
            ISysEmail emailService,
            IOptions<CareerTalentPipelineOptions> optionsAccessor)
        {
            _emailService = emailService;
            _options = optionsAccessor.Value;
        }

        public List<string> GetQuestions() => _options.Questions;

        public async Task<bool> SubmitCareerTalentPipeline(
            CareerTalentPipelineDto careerTalentPipelineDto,
            HiringManagerDto hiringManagerDto)
        {
            if (careerTalentPipelineDto == null) { throw new ArgumentNullException(nameof(careerTalentPipelineDto)); }
            if (hiringManagerDto == null) { throw new ArgumentNullException(nameof(hiringManagerDto)); }

            var responseEmailSuccess = await SubmitResponseEmail(careerTalentPipelineDto.Email);
            var ccEmailSuccess = await SubmitCcEmail(careerTalentPipelineDto, hiringManagerDto);

            return responseEmailSuccess && ccEmailSuccess;
        }

        private async Task<bool> SubmitResponseEmail(string email)
            => await _emailService.SendTemplatedEmailAsync(
                email,
                _options.ResponseEmailTemplateId,
                null,
                Constants.SendGridAccount.Transactional,
                "Thank you");

        private async Task<bool> SubmitCcEmail(
            CareerTalentPipelineDto careerTalentPipelineDto,
            HiringManagerDto hiringManagerDto)
            => await _emailService.SendTemplatedEmailAsync(
                hiringManagerDto.Email,
                _options.ccEmailTemplateId,
                BuildCcEmailTemplateData(careerTalentPipelineDto, hiringManagerDto),
                Constants.SendGridAccount.Transactional,
                "Custom Talent Pipeline");

        private static object BuildCcEmailTemplateData(
            CareerTalentPipelineDto careerTalentPipelineDto,
            HiringManagerDto hiringManagerDto)
            => new
            {
                hiringManagerName = hiringManagerDto.FirstName,
                hiringManagerCompany = hiringManagerDto.CompanyName,
                hiringManagerEmail = hiringManagerDto.Email,
                hiringManagerPhoneNumber = careerTalentPipelineDto.PhoneNumber,
                preferences = careerTalentPipelineDto.Preferences.ToString(),
                questions = careerTalentPipelineDto.Questions
                    .Select(q => new
                    {
                        question = q.Key,
                        answer = q.Value
                    })
            };
    }
}
