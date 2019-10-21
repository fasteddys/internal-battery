using System;
using AutoMapper;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using UpDiddyLib.Helpers;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using static UpDiddyLib.Helpers.Constants;

using com.traitify.net.TraitifyLibrary;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class TraitifyService : ITraitifyService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        private readonly ISubscriberService _subscriberService;
        private ILogger<TraitifyService> _logger;
        private readonly ZeroBounceApi _zeroBounceApi;
        private readonly IConfiguration _config;
        private readonly ISysEmail _sysEmail;
        private readonly string _publicKey;
        private readonly string _host;
        private readonly string _resultUrl;
        private readonly com.traitify.net.TraitifyLibrary.Traitify _traitify;


        public TraitifyService(IRepositoryWrapper repositoryWrapper,
         IMapper mapper
         , ISubscriberService subscriberService
         , ILogger<TraitifyService> logger
         , IConfiguration config
         , ISysEmail sysEmail)
        {
            string secretKey = config["Traitify:SecretKey"];
            string version = config["Traitify:Version"];
            _publicKey = config["Traitify:PublicKey"];
            _host = config["Traitify:HostUrl"];
            _resultUrl = config["Traitify:ResultUrl"];
            _traitify = new com.traitify.net.TraitifyLibrary.Traitify(_host, _publicKey, secretKey, version);
            _subscriberService = subscriberService;
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _logger = logger;
            _zeroBounceApi = new ZeroBounceApi(config, repositoryWrapper, logger);
            _config = config;

        }


        public async Task<TraitifyDto> StartNewAssesment(TraitifyDto dto)
        {
            string deckId = _config["Traitify:DeckId"];
            var newAssessment = _traitify.CreateAssesment(deckId);
            dto.AssessmentId = newAssessment.id;
            dto.DeckId = deckId;
            dto.PublicKey = _publicKey;
            dto.Host = _host;
            await CreateNewAssessment(dto);
            return dto;
        }

        public async Task<TraitifyDto> GetByAssessmentId(string assessmentId)
        {
            var result = await _repositoryWrapper.TraitifyRepository.GetByAssessmentId(assessmentId);
            return _mapper.Map<TraitifyDto>(result);
        }

        public async Task<TraitifyDto> GetAssessment(string assessmentId)
        {
            try
            {
                var assessment = _traitify.GetAssessment(assessmentId);
                TraitifyDto dto = new TraitifyDto()
                {
                    AssessmentId = assessmentId,
                    PublicKey = _publicKey,
                    Host = _host
                };
                if(assessment.completed_at != null)
                {
                    var completedAssessment = await GetByAssessmentId(assessmentId);
                    dto.IsComplete = true;
                    dto.Email = completedAssessment.Email;                    
                }
                return dto;
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"TraitifyController.GetByAssessmentId: An error occured while attempting retrieve assessment Message: {e.Message}", e);
                return null;
            }
        }

        public async Task<TraitifyDto> CompleteAssessment(string assessmentId)
        {
            try
            {
                var assessment = _traitify.GetAssessment(assessmentId);
                if (assessment.completed_at != null)
                {
                    string results = await GetJsonResults(assessmentId);
                    TraitifyDto dto = new TraitifyDto()
                    {
                        AssessmentId = assessmentId,
                        CompleteDate = DateTime.UtcNow,
                        ResultData = results,
                        ResultLength = results.Length,
                        PublicKey = _publicKey,
                        Host = _host
                    };
                    dto = await CompleteAssessment(dto);
                    dynamic payload = ProcessResult(results);
                    //await SendCompletionEmail(dto.Email, payload);
                    return dto;
                }
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"TraitifyController.CompleteAssessment: An error occured while attempting to complete the assessment  Message: {e.Message}", e);
                return null;
            }
            return null;
        }



        public async Task CreateNewAssessment(TraitifyDto dto)
        {
            Subscriber subscriber = null;
            if (dto.SubscriberGuid != null)
            {
                subscriber = await _subscriberService.GetBySubscriberGuid(dto.SubscriberGuid.Value);
            }
            UpDiddyApi.Models.Traitify traitify = new UpDiddyApi.Models.Traitify()
            {
                Subscriber = subscriber != null ? subscriber : null,
                SubscriberId = subscriber != null ? subscriber.SubscriberId : (int?)null,
                TraitifyGuid = Guid.NewGuid(),
                CreateDate = DateTime.UtcNow,
                AssessmentId = dto.AssessmentId,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                DeckId = dto.DeckId
            };
            await _repositoryWrapper.TraitifyRepository.Create(traitify);
            await _repositoryWrapper.TraitifyRepository.SaveAsync();
        }

        private async Task<TraitifyDto> CompleteAssessment(TraitifyDto dto)
        {
            UpDiddyApi.Models.Traitify traitify = await _repositoryWrapper.TraitifyRepository.GetByAssessmentId(dto.AssessmentId);
            traitify.CompleteDate = dto.CompleteDate;
            traitify.ResultData = dto.ResultData;
            traitify.ResultLength = dto.ResultLength;
            dto.Email = traitify.Email;
            await _repositoryWrapper.TraitifyRepository.SaveAsync();
            return dto;
        }

        public async Task CompleteSignup(string assessmentId, int subscriberId)
        {
            UpDiddyApi.Models.Traitify traitify = await _repositoryWrapper.TraitifyRepository.GetByAssessmentId(assessmentId);
            traitify.SubscriberId = subscriberId;
            traitify.ModifyDate = DateTime.UtcNow;
            await _repositoryWrapper.TraitifyRepository.SaveAsync();

            //track user created an account
        }

        private async Task SendCompletionEmail(string sendTo, dynamic result)
        {
            bool? isEmailValid = _zeroBounceApi.ValidateEmail(sendTo);
            if (isEmailValid != null && isEmailValid.Value == true)
            {
                await _sysEmail.SendTemplatedEmailAsync(
                                 sendTo,
                                 _config["SysEmail:Transactional:TemplateIds:PersonalityAssessment-ResultsSummary"],
                                 result,
                                 SendGridAccount.Transactional,
                                 null,
                                 null);
            }
        }

        private async Task<string> GetJsonResults(string assessmentId)
        {
            string apiResponse = string.Empty;
            var url = _resultUrl.Replace("{assessmentId}", assessmentId);
            using (var httpClient = new HttpClient())
            {
                var byteArray = Encoding.ASCII.GetBytes($"{_publicKey}:x");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                using (var response = await httpClient.GetAsync(url))
                {
                    apiResponse = await response.Content.ReadAsStringAsync();
                }
            }
            return apiResponse;
        }

        private dynamic ProcessResult(string rawData)
        {
            var result = JObject.Parse(rawData);
            dynamic jObj = new JObject();
            jObj.blend = (JObject)result["personality_blend"];
            jObj.types = (JArray)result["personality_types"];
            foreach (var type in jObj.types)
            {
                decimal score = (decimal)type["score"];
                type["score"] = Math.Round(score, 0);
            }
            var traitRef = (JArray)result["personality_traits"];
            var traits = new JArray();
            foreach (var trait in traitRef)
            {
                var d = (decimal)trait["score"];
                decimal roundScore = Math.Round(d, 0);
                if (roundScore > 0)
                {
                    trait["score"] = roundScore;
                    traits.Add(trait);
                }
            }
            jObj.traits = traits;
            return jObj;
        }
    }
}
