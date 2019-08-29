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
namespace UpDiddyApi.Controllers
{
    [ApiController]
    public class TraitifyController : ControllerBase
    {
        private readonly ILogger _syslog;
        private readonly Traitify _traitify;
        private IMapper _mapper;
        private readonly IConfiguration _config;
        private readonly ITraitifyService _traitifyService;

        private readonly string _publicKey;
        private readonly string _host;


        public TraitifyController( ITraitifyService traitifyService, 
        ILogger<TrackingController> sysLog,  
        IRepositoryWrapper repositoryWrapper,
         IMapper mapper, 
         IConfiguration config)
        {
            _publicKey = _config["Traitify:PublicKey"];
            _host = _config["Traitify:HostUrl"];
            string secretKey = _config["Traitify:SecretKey"];
            string version = _config["Traitify:Version"];
            _traitify = new Traitify(_host, _publicKey, secretKey, version);
            _syslog = sysLog;
            _config = config;
            _traitifyService = traitifyService;
            _mapper = mapper;
        }
        

        [HttpPost]
        [Route("api/[controller]/new")]
        public async Task<string> StartNewAssesment(TraitifyDto dto) {

                 string deckId = _config["Traitify:DeckId"];
                 var newAssessment = _traitify.CreateAssesment(deckId);
                 dto.AssessmentId = newAssessment.id;
                 dto.DeckId = deckId;
                 dto.PublicKey = _publicKey;
                 dto.Host = _host;
                 await _traitifyService.CreateNewAssessment(dto);
                 return newAssessment.id;
        }
    }
}