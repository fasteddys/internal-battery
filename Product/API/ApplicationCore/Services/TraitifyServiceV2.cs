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
using UpDiddyApi.Helpers;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class TraitifyServiceV2 : ITraitifyServiceV2
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        private ILogger<TraitifyService> _logger;
        private readonly ZeroBounceApi _zeroBounceApi;
        private readonly IConfiguration _config;
        private readonly ISysEmail _sysEmail;
        private readonly string _publicKey;
        private readonly string _host;
        private readonly com.traitify.net.TraitifyLibrary.Traitify _traitify;
        private readonly ITrackingService _trackingService;

        public TraitifyServiceV2(IRepositoryWrapper repositoryWrapper,
         IMapper mapper
         , ILogger<TraitifyService> logger
         , IConfiguration config
         , ISysEmail sysEmail
         , ITrackingService trackingService)
        {
            string secretKey = config["Traitify:SecretKey"];
            string version = config["Traitify:Version"];
            _publicKey = config["Traitify:PublicKey"];
            _host = config["Traitify:HostUrl"];
            _traitify = new com.traitify.net.TraitifyLibrary.Traitify(_host, _publicKey, secretKey, version);
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
                subscriber = await _repositoryWrapper.SubscriberRepository.GetSubscriberByGuidAsync(subscriberGuid);
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
                    string results = await TraitifyHelper.GetJsonResults(assessmentId, _config);
                    UpDiddyApi.Models.Traitify traitify = await _repositoryWrapper.TraitifyRepository.GetByAssessmentId(assessmentId);
                    if (traitify.CompleteDate != null)
                        throw new TraitifyException("The assessment has already been completed.");
                    traitify.CompleteDate = DateTime.UtcNow;
                    traitify.ResultData = results;
                    traitify.ResultLength = results.Length;
                    if (traitify.SubscriberId != null)
                    {
                        await TraitifyHelper.SendCompletionEmail(assessmentId, traitify.Email, _repositoryWrapper, _sysEmail, _config, _zeroBounceApi);
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

        #endregion

    }
}
