﻿using System;
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
using System.Security.Claims;

namespace UpDiddyApi.Controllers
{
    [Route("api/[controller]")]
    public class ResumeController : Controller
    {
        private ISovrenAPI _sovrenApi;
        private UpDiddyDbContext _db;

        public ResumeController(ISovrenAPI sovrenApi, UpDiddyDbContext db)
        {
            this._sovrenApi = sovrenApi;
            this._db = db;
        }

        // POST api/<controller>/upload/subscriber/{SubscriberGuid}
        /// <summary>
        /// Resume Upload Endpoint that takes a resume upload and submits it to sovren to get HRXML and saves it in the
        /// subscriber profile staging store.
        /// </summary>
        /// <param name="resume">File uploaded as part of form-data with key name of "resume"</param>
        /// <returns>string HRXML</returns>
        [Authorize]
        [HttpPost]
        [Route("upload")]
        public async Task<IActionResult> Upload(IFormFile resume)
        {
            Guid subscriberGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);

            // todo: research and implement a better way to handle soft deletes then manual checks everywhere
            Subscriber subscriber = _db.Subscriber
                .Where(s => s.SubscriberGuid == subscriberGuid && s.IsDeleted == 0)
                .FirstOrDefault();

            if (subscriber == null)
                return NotFound();

            string base64String = null;
            using (var stream = new MemoryStream())
            {
                resume.CopyTo(stream);
                var fileBytes = stream.ToArray();
                base64String = Convert.ToBase64String(fileBytes);
            }

            // todo: submit as background job, maybe depends on onboarding flow
            String parsedDocument = await _sovrenApi.SubmitResumeAsync(base64String);

            // todo: verify subscriber guid permissions and data
            SubscriberProfileStagingStore.Save(_db, subscriber, Constants.DataSource.Sovren, Constants.DataFormat.Xml, parsedDocument);

            // run job to import user profile data 
            BackgroundJob.Enqueue<ScheduledJobs>(j => j.ImportSubscriberProfileData(subscriberGuid));

            return Ok(parsedDocument);
        }
    }
}