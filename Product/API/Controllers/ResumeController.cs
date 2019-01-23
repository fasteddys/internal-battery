using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.Models;
using UpDiddyLib.Helpers;
using Hangfire;
using UpDiddyApi.Workflow;
using UpDiddyLib.Dto;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace UpDiddyApi.Controllers
{
    [Route("api/[controller]")]
    public class ResumeController : Controller
    {
        private ISovrenAPI _sovrenApi;
        private UpDiddyDbContext _db;
        protected internal ILogger _syslog = null;
        private ICloudStorage _cloudStorage;

        public ResumeController(ISovrenAPI sovrenApi, UpDiddyDbContext db, ILogger<ResumeController> sysLog, ICloudStorage cloudStorage)
        {
            this._sovrenApi = sovrenApi;
            this._db = db;
            this._syslog = sysLog;
            this._cloudStorage = cloudStorage;
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
        public async Task<IActionResult> Upload(IFormFile resume, bool parseResume = false)
        {
            Guid subscriberGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);

            // todo: research and implement a better way to handle soft deletes then manual checks everywhere
            Subscriber subscriber = _db.Subscriber
                .Include(s => s.SubscriberFile)
                .Where(s => s.SubscriberGuid == subscriberGuid && s.IsDeleted == 0)
                .FirstOrDefault();

            if (subscriber == null)
                return NotFound(new BasicResponseDto{ StatusCode = "404", Description = "Subscriber not found in the system." });

            SubscriberFile subscriberFileResume = new SubscriberFile
            {
                BlobName = await _cloudStorage.UploadFileAsync(String.Format("{0}/{1}/", subscriberGuid, "resume"), resume.FileName, resume.OpenReadStream()),
                ModifyGuid = subscriberGuid,
                CreateGuid = subscriberGuid,
                CreateDate = DateTime.UtcNow,
                ModifyDate = DateTime.UtcNow
            };

            // check to see if file is already in the system, if there is a file in the system in already then delete it
            // todo: refactor as part of multiple file upload/management system
            // todo: move logic to OnDelete event or somewhere centralized and run as a transaction somehow
            if(subscriber.SubscriberFile.Count > 0)
            {
                SubscriberFile oldFile = subscriber.SubscriberFile.Last();
                await _cloudStorage.DeleteFileAsync(oldFile.BlobName);
                subscriber.SubscriberFile.Remove(oldFile);
            }

            subscriber.SubscriberFile.Add(subscriberFileResume);
            _db.SaveChanges();

            if(parseResume)
                BackgroundJob.Enqueue<ScheduledJobs>(j => j.ImportSubscriberProfileDataAsync(subscriberFileResume));

            return Ok(new BasicResponseDto { StatusCode = "200", Description = "Success!" });
        }
    }
}