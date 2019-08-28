using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using AutoMapper;
using UpDiddyLib.Dto;
using com.traitify.net.TraitifyLibrary;
using System.Collections.Generic;

namespace UpDiddyApi.Controllers
{
    [ApiController]
    public class TraitifyController : ControllerBase
    {
        private readonly ILogger _syslog;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly Traitify _traitify;
        private IMapper _mapper;



        public TraitifyController( ILogger<TrackingController> sysLog,  IRepositoryWrapper repositoryWrapper, IMapper mapper)
        {
            _syslog = sysLog;
            _repositoryWrapper = repositoryWrapper;
            _traitify = new Traitify("https://api.traitify.com", "3d731f347b674c7da1c55b25aa172314", "cb156aed701d491f9a8114896b5d9a1f", "v1");
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
                Slides = _mapper.Map<List<TraitifySlideDto>>(slides)
            };
            return dto;
        }
    }
}