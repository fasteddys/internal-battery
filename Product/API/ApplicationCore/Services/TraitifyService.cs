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
        private readonly ITrackingService _trackingService;

        public TraitifyService(IRepositoryWrapper repositoryWrapper,
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
                if (assessment.completed_at != null)
                {
                    var completedAssessment = await GetByAssessmentId(assessmentId);
                    dto.SubscriberGuid = completedAssessment.SubscriberGuid;
                    dto.IsComplete = true;
                    dto.IsRegistered = completedAssessment.SubscriberId != null ? true : false;
                    dto.Email = completedAssessment.Email;
                    dto.FirstName = completedAssessment.FirstName;
                    dto.LastName = completedAssessment.LastName;
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
                    UpDiddyApi.Models.Traitify traitify = await _repositoryWrapper.TraitifyRepository.GetByAssessmentId(assessmentId);
                    traitify.CompleteDate = DateTime.UtcNow;
                    traitify.ResultData = results;
                    traitify.ResultLength = results.Length;
                    if (traitify.SubscriberId != null)
                    {
                        await SendCompletionEmail(assessmentId, traitify.Email);
                    }
                    await _repositoryWrapper.TraitifyRepository.SaveAsync();
                    TraitifyDto dto = new TraitifyDto
                    {
                        Email = traitify.Email
                    };
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

        public async Task CompleteSignup(string assessmentId, Guid subscriberGuid)
        {
            var subscriber = _repositoryWrapper.SubscriberRepository.GetSubscriberByGuid(subscriberGuid);
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

        private async Task SendCompletionEmail(string assessmentId, string sendTo)
        {
            string results = await GetJsonResults(assessmentId);
            dynamic payload = await ProcessResult(results);
            bool? isEmailValid = _zeroBounceApi.ValidateEmail(sendTo);
            if (isEmailValid != null && isEmailValid.Value == true)
            {
                await _sysEmail.SendTemplatedEmailAsync( 
                                _logger,
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

    }
}
