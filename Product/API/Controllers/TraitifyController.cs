using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using AutoMapper;
using UpDiddyLib.Dto;
using com.traitify.net.TraitifyLibrary;
using Microsoft.Extensions.Configuration;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using System.Threading.Tasks;
using System;
using UpDiddyLib.Helpers;
using static UpDiddyLib.Helpers.Constants;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using UpDiddyApi.ApplicationCore.Services;

namespace UpDiddyApi.Controllers
{
    [ApiController]
    public class TraitifyController : ControllerBase
    {

        private readonly ITraitifyService _traitifyService;


        public TraitifyController(ITraitifyService traitifyService)
        {

            _traitifyService = traitifyService;

        }


        [HttpPost]
        [Route("api/[controller]/new")]
        public async Task<TraitifyDto> StartNewAssesment(TraitifyDto dto)
        {
            return await _traitifyService.StartNewAssesment(dto);
        }

        [HttpGet]
        [Route("api/[controller]/{assessmentId:length(36)}")]
        public async Task<TraitifyDto> GetAssessment(string assessmentId)
        {
            return await _traitifyService.GetAssessment(assessmentId);
        }

        [HttpGet]
        [Route("api/[controller]/complete/{assessmentId:length(36)}")]
        public async Task<TraitifyDto> CompleteAssessment(string assessmentId)
        {
            return await _traitifyService.CompleteAssessment(assessmentId);
        }
    }
}

