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
using UpDiddyApi.ApplicationCore.Interfaces.Business;

namespace UpDiddyApi.Controllers
{
    [Route("api/[controller]")]
    public class ResumeController : Controller
    {
        private UpDiddyDbContext _db;
        private ISubscriberService _subscriberService;
        protected internal ILogger _syslog = null;

        public ResumeController(UpDiddyDbContext db, ISubscriberService subscriberService, ILogger<ResumeController> sysLog)
        {
            this._db = db;
            this._syslog = sysLog;
            this._subscriberService = subscriberService;
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
                return NotFound(new BasicResponseDto{ StatusCode = 404, Description = "Subscriber not found in the system." });

            await _subscriberService.AddResumeAsync(subscriber, resume.FileName, resume.OpenReadStream(), parseResume);

            return Ok(new BasicResponseDto { StatusCode = 200, Description = "Success!" });
        }
    }
}