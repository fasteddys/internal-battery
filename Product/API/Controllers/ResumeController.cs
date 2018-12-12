using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using UpDiddyApi.Business.Resume;
using System.Net.Http;
using UpDiddyApi.Models;
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
        /// <param name="SubscriberGuid">User Subscriber Guid</param>
        /// <param name="resume">File uploaded as part of form-data with key name of "resume"</param>
        /// <returns>string HRXML</returns>
        [Authorize]
        [HttpPost]
        [Route("upload/subscriber/{SubscriberGuid}")]
        public async Task<IActionResult> Upload(Guid SubscriberGuid, IFormFile resume)
        {
            Subscriber subscriber = _db.Subscriber
                .Where(s => s.SubscriberGuid == SubscriberGuid && s.IsDeleted == 0)
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
            // todo: research and implement a better way to handle soft deletes then manual checks everywhere
            SubscriberProfileStagingStore.Save(_db, subscriber, Sovren.Name, SubscriberProfileStagingStore.DataFormatXml, parsedDocument);
            return Ok(parsedDocument);
        }
    }
}