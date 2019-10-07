using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using UpDiddyLib.Dto;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using System.Net.Http;

namespace UpDiddy.Controllers
{

    [Route("api/[controller]")]
    public class FileController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IFileDownloadTrackerService _fileDownloadTrackerService;

        public FileController(IConfiguration configuration, IFileDownloadTrackerService fileDownloadTrackerService)
        {
            _configuration = configuration;
            _fileDownloadTrackerService = fileDownloadTrackerService;
        }
        [HttpGet]
        [Route("gated/{fileDownloadTrackerGuid}")]
        public async Task<IActionResult> GetFile(Guid fileDownloadTrackerGuid)
        {
            FileDto fileDto = new FileDto();
            try
            {
                FileDownloadTrackerDto trackerDto = await _fileDownloadTrackerService.GetByFileDownloadTrackerGuid(fileDownloadTrackerGuid);
                if (trackerDto.FileDownloadAttemptCount <= trackerDto.MaxFileDownloadAttemptsPermitted)
                {
                    using (var client = new HttpClient())
                    {
                        using (var result = await client.GetAsync(trackerDto.SourceFileCDNUrl))
                        {
                            if (result.IsSuccessStatusCode)
                            {

                                byte[] payload = await result.Content.ReadAsByteArrayAsync();
                                if (payload != null && payload.Length > 0)
                                {
                                    fileDto.FileName = result.Content.Headers.ContentDisposition.FileName.Replace("\"",string.Empty);
                                    fileDto.MimeType = result.Content.Headers.ContentType.MediaType;
                                    fileDto.Payload = payload;
                                }
                                else
                                {
                                    return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = "The requested file is invalid" });
                                }
                            }
                            else
                            {
                                return BadRequest(new BasicResponseDto() { StatusCode = 400, Description = "Failed to retrieve file from remote source." });
                            }

                        }
                    }
                }
                else
                {
                    return BadRequest(new BasicResponseDto() { StatusCode = 403, Description = "Maximum file download attempt has been exceeded." });
                }
            }
            catch (Exception e)
            {
                    return BadRequest(new BasicResponseDto() { StatusCode = 500, Description = "Ops something went wrong" });

            }

            return Ok(fileDto);
        }
    }
}

