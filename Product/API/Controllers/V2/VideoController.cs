using Microsoft.AspNetCore.Mvc;
using UpDiddyApi.ApplicationCore.Interfaces.Business;

namespace UpDiddyApi.Controllers.V2
{
    [Route("/V2/[controller]/")]
    [ApiController]
    public class VideoController : BaseApiController
    {
        private readonly IVideoService _videoService;

        public VideoController(IVideoService videoService)
        {
            _videoService = videoService;
        }
    }
}
