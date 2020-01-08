using System;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
namespace UpDiddyApi.Controllers
{
    [Route("/V2/[controller]")]
    public class FileController : ControllerBase
    {
        private readonly IFileDownloadTrackerService _fileDownloadTrackerService;

        public FileController(IFileDownloadTrackerService fileDownloadTrackerService)
        {
            _fileDownloadTrackerService = fileDownloadTrackerService;
        }

        [HttpGet]
        [Route("gated/{fileDownloadTrackerGuid}")]
        public async Task<IActionResult> GetGatedFile(Guid fileDownloadTrackerGuid)
        {
            string fileUrl = await _fileDownloadTrackerService.GetFileUrlByFileDownloadTrackerGuid(fileDownloadTrackerGuid);
            return Ok(fileUrl);
        }
    }
}

