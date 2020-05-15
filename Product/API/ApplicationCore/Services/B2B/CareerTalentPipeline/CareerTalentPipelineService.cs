using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business.B2B;
using UpDiddyLib.Domain.Models.B2B;
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

        public async Task<bool> SubmitCareerTalentPipeline(CareerTalentPipelineDto careerTalentPipelineDto)
        {
            if (careerTalentPipelineDto == null) { throw new ArgumentNullException(nameof(careerTalentPipelineDto)); }

            var responseEmailSuccess = await SubmitResponseEmail(careerTalentPipelineDto.Email);
            var ccEmailSuccess = await SubmitCcEmail(careerTalentPipelineDto);

            return responseEmailSuccess && ccEmailSuccess;
        }

        private async Task<bool> SubmitResponseEmail(string email)
            => await _emailService.SendTemplatedEmailAsync(
                email,
                _options.ResponseEmailTemplateId,
                null, //TODO: build template data
                Constants.SendGridAccount.Transactional,
                "Thank you");

        private async Task<bool> SubmitCcEmail(CareerTalentPipelineDto careerTalentPipelineDto)
            => await _emailService.SendTemplatedEmailAsync(
                _options.ccEmail,
                _options.ccEmailTemplateId,
                BuildCcEmailTemplateData(careerTalentPipelineDto),
                Constants.SendGridAccount.Transactional,
                "Custom Talent Pipeline");

        private static object BuildCcEmailTemplateData(CareerTalentPipelineDto careerTalentPipelineDto)
            => new
            {
                careerTalentPipelineDto.Email,
                careerTalentPipelineDto.PhoneNumber,
                careerTalentPipelineDto.Preferences,
                Questions = careerTalentPipelineDto.Questions
                    .Select(q => new
                    {
                        Question = q.Key,
                        Answer = q.Value
                    })
            };
    }
}
