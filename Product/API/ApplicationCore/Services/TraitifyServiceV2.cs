using System.Security.Cryptography.X509Certificates;
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
using EntityTypeConst = UpDiddyLib.Helpers.Constants.EventType;
using UpDiddyLib.Domain.Models;
using UpDiddyApi.ApplicationCore.Exceptions;
namespace UpDiddyApi.ApplicationCore.Services
{
    public class TraitifyServiceV2 : ITraitifyServiceV2
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
        private readonly ITrackingService _trackingService;

        public TraitifyServiceV2(IRepositoryWrapper repositoryWrapper,
         IMapper mapper
         , ISubscriberService subscriberService
         , ILogger<TraitifyService> logger
         , IConfiguration config
         , ISysEmail sysEmail
         , ITrackingService trackingService)
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
            _sysEmail = sysEmail;
            _trackingService = trackingService;
        }

        public async Task<TraitifyResponseDto> StartNewAssesment(TraitifyRequestDto dto, Guid subscriberGuid)
        {
            //If subscriber guid is not empty, it means an existing user is taking the assessment therefore the dto is not required.
            Subscriber subscriber = null;
            if (subscriberGuid != Guid.Empty)
            {
                subscriber = await _subscriberService.GetBySubscriberGuid(subscriberGuid);
            }
            else
            {
                if (dto == null)
                {
                    throw new NullReferenceException("TraitifyRequestDto cannot be null.");
                }
                else
                {
                    if (dto.FirstName == null || dto.LastName == null || dto.Email == null)
                        throw new NullReferenceException("FirstName, LastName, and Email cannot be null or empty.");
                }
            }
            string deckId = _config["Traitify:DeckId"];
            var newAssessment = _traitify.CreateAssesment(deckId);
            TraitifyResponseDto responseDto = new TraitifyResponseDto()
            {
                AssessmentId = newAssessment.id,
                PublicKey = _publicKey,
                Host = _host
            };
            dto.AssessmentId = newAssessment.id;
            await CreateNewAssessment(dto, subscriber);
            return responseDto;
        }

        public async Task CompleteAssessment(string assessmentId)
        {
            if (string.IsNullOrEmpty(assessmentId))
            {
                throw new NullReferenceException("AssessmentId cannot be null or empty.");
            }
            try
            {
                var assessment = _traitify.GetAssessment(assessmentId);
                if (assessment.completed_at != null)
                {
                    string results = await GetJsonResults(assessmentId);
                    UpDiddyApi.Models.Traitify traitify = await _repositoryWrapper.TraitifyRepository.GetByAssessmentId(assessmentId);
                    if (traitify.CompleteDate != null)
                        throw new TraitifyException("The assessment has already been completed.");
                    traitify.CompleteDate = DateTime.UtcNow;
                    traitify.ResultData = results;
                    traitify.ResultLength = results.Length;
                    if (traitify.SubscriberId != null)
                    {
                        await SendCompletionEmail(assessmentId, traitify.Email);
                    }
                    await _repositoryWrapper.TraitifyRepository.SaveAsync();
                }
                else
                {
                    throw new TraitifyException("The assessment is not complete.");
                }
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"TraitifyController.CompleteAssessment: An error occured while attempting to complete the assessment  Message: {e.Message}", e);
                throw new TraitifyException(e.Message);
            }
        }

        public async Task CompleteSignup(string assessmentId, Subscriber subscriber)
        {
            try
            {
                UpDiddyApi.Models.Traitify traitify = await _repositoryWrapper.TraitifyRepository.GetByAssessmentId(assessmentId);
                traitify.SubscriberId = subscriber.SubscriberId;
                traitify.ModifyDate = DateTime.UtcNow;
                traitify.Email = subscriber.Email;
                await _repositoryWrapper.TraitifyRepository.SaveAsync();
                Models.Action action = await _repositoryWrapper.ActionRepository.GetByNameAsync(UpDiddyLib.Helpers.Constants.Action.TraitifyAccountCreation);
                EntityType entityType = await _repositoryWrapper.EntityTypeRepository.GetByNameAsync(EntityTypeConst.TraitifyAssessment);
                await _trackingService.TrackSubscriberAction(subscriber.SubscriberId, action, entityType, traitify.Id);
                await SendCompletionEmail(assessmentId, traitify.Email);
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"TraitifyController.CompleteSignup: An error occured while attempting to complete signup Message: {e.Message}", e);
            }
        }


        #region Private Functions
        private async Task CreateNewAssessment(TraitifyRequestDto dto, Subscriber subscriber)
        {

            UpDiddyApi.Models.Traitify traitify = new UpDiddyApi.Models.Traitify()
            {
                Subscriber = subscriber != null ? subscriber : null,
                SubscriberId = subscriber != null ? subscriber.SubscriberId : (int?)null,
                TraitifyGuid = Guid.NewGuid(),
                CreateDate = DateTime.UtcNow,
                AssessmentId = dto.AssessmentId,
                FirstName = subscriber == null ? dto.FirstName : subscriber.FirstName,
                LastName = subscriber == null ? dto.LastName : subscriber.LastName,
                Email = subscriber == null ? dto.Email : subscriber.Email,
                DeckId = _config["Traitify:DeckId"]
            };
            await _repositoryWrapper.TraitifyRepository.Create(traitify);
            await _repositoryWrapper.TraitifyRepository.SaveAsync();
        }

        private async Task SendCompletionEmail(string assessmentId, string sendTo)
        {
            string results = await GetJsonResults(assessmentId);
            dynamic payload = await ProcessResult(results);
            bool? isEmailValid = _zeroBounceApi.ValidateEmail(sendTo);
            if (isEmailValid != null && isEmailValid.Value == true)
            {
                await _sysEmail.SendTemplatedEmailAsync(
                                 sendTo,
                                 _config["SysEmail:Transactional:TemplateIds:PersonalityAssessment-ResultsSummary"],
                                 payload,
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

        private async Task<dynamic> ProcessResult(string rawData)
        {
            var result = JObject.Parse(rawData);
            dynamic jObj = new JObject();
            jObj.blend = (JObject)result["personality_blend"];
            jObj.types = (JArray)result["personality_types"];
            jObj.courses = await GetSuggestedCourses(jObj.blend);
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

        private async Task<dynamic> GetSuggestedCourses(dynamic blend)
        {
            string type1 = blend["personality_type_1"].SelectToken("name").Value;
            string type2 = blend["personality_type_2"].SelectToken("name").Value;
            var mapping = await _repositoryWrapper.TraitifyCourseTopicBlendMappingRepository.GetByPersonalityTypes(type1, type2);
            var courses = new JArray();
            string baseUrl = _config["Environment:BaseUrl"];
            baseUrl = baseUrl.Substring(0, (baseUrl.Length - 1));
            if (mapping != null)
            {
                if (!string.IsNullOrEmpty(mapping.TopicOneName))
                {
                    dynamic course1 = new JObject();
                    course1.imgUrl = mapping.TopicOneImgUrl;
                    course1.courseUrl = baseUrl + mapping.TopicOneUrl;
                    courses.Add(course1);
                }

                if (!string.IsNullOrEmpty(mapping.TopicTwoName))
                {
                    dynamic course2 = new JObject();
                    course2.imgUrl = mapping.TopicTwoImgUrl;
                    course2.courseUrl = baseUrl + mapping.TopicTwoUrl;
                    courses.Add(course2);
                }

                if (!string.IsNullOrEmpty(mapping.TopicThreeName))
                {
                    dynamic course3 = new JObject();
                    course3.imgUrl = mapping.TopicThreeImgUrl;
                    course3.courseUrl = baseUrl + mapping.TopicThreeUrl;
                    courses.Add(course3);
                }
            }
            return courses;
        }

        #endregion

    }
}
