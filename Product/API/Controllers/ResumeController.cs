using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using UpDiddyApi.Business.Resume;
using UpDiddyApi.Models;
using UpDiddy.Helpers;
using Hangfire;
using UpDiddyApi.Workflow;
using UpDiddyLib.Dto;
using Microsoft.Extensions.Logging;

namespace UpDiddyApi.Controllers
{
    [Route("api/[controller]")]
    public class ResumeController : Controller
    {
        private ISovrenAPI _sovrenApi;
        private UpDiddyDbContext _db;
        protected internal ILogger _syslog = null;

        public ResumeController(ISovrenAPI sovrenApi, UpDiddyDbContext db, ILogger<ResumeController> sysLog)
        {
            this._sovrenApi = sovrenApi;
            this._db = db;
            this._syslog = sysLog;
        }

        /// <summary>
        /// Resume Upload Endpoint that takes a resume upload and submits it to sovren to get HRXML and saves it in the
        /// subscriber profile staging store.
        /// </summary>
        /// <param name="resumeDto">The data transfer object which contains a subscriber guid and a base 64 encoded string representation of a resume</param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("upload")]
        public async Task<IActionResult> Upload([FromBody] ResumeDto resumeDto)
        {
            BasicResponseDto basicResponseDto = new BasicResponseDto()
            {
                StatusCode = "100",
                Description = "Initialized"
            };

            try
            {
                if (resumeDto != null && resumeDto.SubscriberGuid != Guid.Empty && !string.IsNullOrWhiteSpace(resumeDto.Base64EncodedResume))
                {

                    // todo: research and implement a better way to handle soft deletes then manual checks everywhere
                    Subscriber subscriber = _db.Subscriber
                        .Where(s => s.SubscriberGuid == resumeDto.SubscriberGuid && s.IsDeleted == 0)
                        .FirstOrDefault();

                    if (subscriber != null)
                    {
                        // Queue job as background process 
                        BackgroundJob.Enqueue<ScheduledJobs>(j => j.ImportSubscriberProfileData(resumeDto, subscriber));

                        // indicate that a background job is being processed
                        basicResponseDto.StatusCode = "Processing";
                        basicResponseDto.Description = "Scheduled job to import profile data is being processed.";
                    }
                    else
                    {
                        basicResponseDto.StatusCode = "NotFound";
                        basicResponseDto.Description = "Subscriber does not exist.";
                    }
                }
                else
                {
                    basicResponseDto.StatusCode = "BadRequest";
                    basicResponseDto.Description = "The parameter supplied is invalid.";
                }
            }
            catch (Exception ex)
            {

                basicResponseDto.StatusCode = "InternalServerError";
                basicResponseDto.Description = ex.Message;
                _syslog.Log(LogLevel.Error, $"ResumeController.Upload: Parameters subscriber= {(resumeDto == null ? string.Empty : resumeDto.SubscriberGuid.ToString())} resume= {(resumeDto == null ? string.Empty : resumeDto.Base64EncodedResume)} ");
            }

            return Ok(basicResponseDto);
        }
    }
}