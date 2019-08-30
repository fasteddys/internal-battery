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
        private ILogger<TraitifyController> _logger { get; set; }


        public TraitifyController(ITraitifyService traitifyService,
         IRepositoryWrapper repositoryWrapper,
         ILogger<TraitifyController> logger,
         IMapper mapper,
         IConfiguration config)
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
        public bool CompleteAssessment(string assessmentId)
        {
            try
            {
                var assessment = _traitify.GetAssessment(assessmentId);
                if (assessment.completed_at != null)
                {
                    var traits = _traitify.GetPersonalityTraits(assessmentId);
                    var types = _traitify.GetPersonalityTypes(assessmentId);
                    string[] result = new string[] { JsonConvert.SerializeObject(traits), JsonConvert.SerializeObject(types) };
                    TraitifyDto dto = new TraitifyDto()
                    {
                        AssessmentId = assessmentId,
                        CompletedAt = DateTime.UtcNow,
                        ResultData = JsonConvert.SerializeObject(result),
                        PublicKey = _publicKey,
                        Host = _host
                    };
                    _traitifyService.CompleteAssessment(dto);
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
    }

}