using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using AutoMapper;
using UpDiddyLib.Dto;
using com.traitify.net.TraitifyLibrary;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using System.Threading.Tasks;
using System;
using UpDiddyApi.Helpers;
using UpDiddyLib.Helpers;
using static UpDiddyLib.Helpers.Constants;
using Newtonsoft.Json.Linq;

using Newtonsoft.Json;
namespace UpDiddyApi.Controllers
{
    [ApiController]
    public class TraitifyController : ControllerBase
    {
        private readonly Traitify _traitify;
        private IMapper _mapper;
        private readonly IConfiguration _config;
        private readonly ITraitifyService _traitifyService;
        private readonly string _publicKey;
        private readonly string _host;
        private ILogger<TraitifyController> _logger;
        private readonly ISysEmail _sysEmail;



        public TraitifyController(ITraitifyService traitifyService,
         IRepositoryWrapper repositoryWrapper,
         ILogger<TraitifyController> logger,
         IMapper mapper,
         IConfiguration config,
         ISysEmail sysEmail)
        {
            string secretKey = config["Traitify:SecretKey"];
            string version = config["Traitify:Version"];
            _publicKey = config["Traitify:PublicKey"];
            _host = config["Traitify:HostUrl"];
            _traitify = new Traitify(_host, _publicKey, secretKey, version);
            _config = config;
            _traitifyService = traitifyService;
            _mapper = mapper;
            _logger = logger;
            _sysEmail = sysEmail;
        }


        [HttpPost]
        [Route("api/[controller]/new")]
        public async Task<TraitifyDto> StartNewAssesment(TraitifyDto dto)
        {
            string deckId = _config["Traitify:DeckId"];
            var newAssessment = _traitify.CreateAssesment(deckId);
            dto.AssessmentId = newAssessment.id;
            dto.DeckId = deckId;
            dto.PublicKey = _publicKey;
            dto.Host = _host;
            await _traitifyService.CreateNewAssessment(dto);
            return dto;
        }

        [HttpGet]
        [Route("api/[controller]/{assessmentId}")]
        public TraitifyDto GetByAssessmentId(string assessmentId)
        {
            try
            {
                var assessment = _traitify.GetAssessment(assessmentId);
                if (assessment.completed_at == null)
                {
                    TraitifyDto dto = new TraitifyDto()
                    {
                        AssessmentId = assessmentId,
                        CompletedAt = null,
                        PublicKey = _publicKey,
                        Host = _host
                    };
                    return dto;
                }
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"TraitifyController.GetByAssessmentId: An error occured while attempting retrieve assessment Message: {e.Message}", e);
                return null;
            }
            return null;
        }

        [HttpGet]
        [Route("api/[controller]/complete/{assessmentId}")]
        public async Task<bool> CompleteAssessment(string assessmentId)
        {
            try
            {
                var assessment = _traitify.GetAssessment(assessmentId);
                if (assessment.completed_at != null)
                {
                    dynamic results = new JObject();
                    var types = _traitify.GetPersonalityTypes(assessmentId);
                    var traits = _traitify.GetPersonalityTraits(assessmentId);
                    results.traits = JArray.FromObject(traits);
                    results.types = JArray.FromObject(types.personality_types);
                    results.blend = JObject.FromObject(types.personality_blend);
                    TraitifyDto dto = new TraitifyDto()
                    {
                        AssessmentId = assessmentId,
                        CompletedAt = DateTime.UtcNow,
                        ResultData = results,
                        PublicKey = _publicKey,
                        Host = _host
                    };
                    dto = await _traitifyService.CompleteAssessment(dto);
                    await SendCompletionEmail(dto.Email, results);
                    return true;
                }
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"TraitifyController.CompleteAssessment: An error occured while attempting to complete the assessment  Message: {e.Message}", e);
                return false;
            }
            return false;
        }

        private async Task SendCompletionEmail(string sendTo, dynamic result)
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

}