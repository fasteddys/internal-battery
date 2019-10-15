using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using UpDiddyLib.Dto;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using System.Net.Http;
using UpDiddyLib.Helpers;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using Microsoft.Extensions.Logging;

namespace UpDiddy.Controllers
{
    [Route("api/[controller]")]
    public class FileController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IFileDownloadTrackerService _fileDownloadTrackerService;
        private readonly ITrackingService _trackingService;
        private readonly ILogger<FileController> _logger;

        public FileController(IConfiguration configuration,
        ILogger<FileController> logger,
        ISysEmail sysEmail,
        IFileDownloadTrackerService fileDownloadTrackerService,
        ITrackingService trackingService,
        IRepositoryWrapper repositoryWrapper)
        {
            _configuration = configuration;
            _fileDownloadTrackerService = fileDownloadTrackerService;
            _trackingService = trackingService;
            _logger = logger;
        }

        [HttpGet]
        [Route("gated/{fileDownloadTrackerGuid}")]
        public async Task<IActionResult> GetGatedFile(Guid fileDownloadTrackerGuid)
        {
            FileDto fileDto = new FileDto();
            try
            {
                FileDownloadTrackerDto trackerDto = await _fileDownloadTrackerService.GetByFileDownloadTrackerGuid(fileDownloadTrackerGuid);             
                if (trackerDto == null)
                {
                    fileDto.ErrorMessage = "The requested file is invalid.";
                }
                if ((trackerDto.MaxFileDownloadAttemptsPermitted != null && trackerDto.FileDownloadAttemptCount <= trackerDto.MaxFileDownloadAttemptsPermitted) || trackerDto.MaxFileDownloadAttemptsPermitted == null)
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
                                    fileDto.FileName = result.Content.Headers.ContentDisposition.FileName.Replace("\"", string.Empty);
                                    fileDto.MimeType = result.Content.Headers.ContentType.MediaType;
                                    fileDto.Payload = payload;
                                    trackerDto.FileDownloadAttemptCount++;
                                    trackerDto.MostrecentfiledownloadAttemptinUtc = DateTime.UtcNow;
                                }
                                else
                                {
                                    fileDto.ErrorMessage = "The requested file is invalid. (Source Invalid)";
                                }
                            }
                            else
                            {
                                fileDto.ErrorMessage = "Failed to retrieve the file from remote source.";
                            }
                        }
                    }
                }
                else
                {
                    fileDto.ErrorMessage = "The file has been downloaded maximum amount of times or the link has expired.";
                }
                if (string.IsNullOrEmpty(fileDto.ErrorMessage))
                {
                    await _trackingService.TrackingSubscriberFileDownloadAction(trackerDto.SubscriberId, trackerDto.FileDownloadTrackerId);
                    await _fileDownloadTrackerService.Update(trackerDto);
                }
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"FileController.GetGatedFile : Error occured when updating company with message={ex.Message}", ex);
                return StatusCode(500);
            }
            return Ok(fileDto);

        }
    }
}

