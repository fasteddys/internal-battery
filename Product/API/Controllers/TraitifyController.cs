using System.Net.Sockets;
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
using UpDiddyLib.Helpers;
using static UpDiddyLib.Helpers.Constants;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;
using Newtonsoft.Json;
using System.Text;

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
        private readonly string _resultUrl;
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
            _resultUrl = config["Traitify:ResultUrl"];
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
                        CompleteDate = null,
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
                    dto = await _traitifyService.CompleteAssessment(dto);
                    dynamic payload = ProcessResult(results);
                    await SendCompletionEmail(dto.Email, payload);
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

