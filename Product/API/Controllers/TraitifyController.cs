using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using AutoMapper;
using UpDiddyLib.Dto;
using com.traitify.net.TraitifyLibrary;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace UpDiddyApi.Controllers
{
    [ApiController]
    public class TraitifyController : ControllerBase
    {
        private readonly ILogger _syslog;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly Traitify _traitify;
        private IMapper _mapper;
        private readonly IConfiguration _config;


        public TraitifyController( ILogger<TrackingController> sysLog,  IRepositoryWrapper repositoryWrapper, IMapper mapper, IConfiguration config)
        {
            _syslog = sysLog;
            _config = config;
            _repositoryWrapper = repositoryWrapper;
            string publicKey = _config["Traitify:PublicKey"];
            string secretKey = _config["Traitify:SecretKey"];
            string hostUrl = _config["Traitify:HostUrl"];
            string version = _config["Traitify:Version"];

            //TODO put get this from config 
            _traitify = new Traitify(hostUrl, publicKey, secretKey, version);

            _mapper = mapper;
        }
        

        [HttpGet]
        [Route("api/[controller]/traitify/new")]
        public TraitifyAssesmentDto StartNewAssesment() {

            var assesment = _traitify.CreateAssesment("career-deck");
            var slides = _traitify.GetSlides(assesment.id);

            TraitifyAssesmentDto dto = new TraitifyAssesmentDto()
            {
                AssesmentId = assesment.id,
            };
            return dto;
        }
    }
}