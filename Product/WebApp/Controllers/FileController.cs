using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using UpDiddyLib.Dto;
using UpDiddy.Api;
using System.Threading.Tasks;

namespace UpDiddy.Controllers
{
    public class FileController : BaseController
    {
        public FileController(IApi api, IConfiguration configuration) : base(api, configuration)
        {

        }

        [HttpGet]
        [Route("/GetFile/")]
        public ActionResult Index(Guid f)
        {
            if (f == Guid.Empty)
            {
                return BadRequest();
            }
            ViewBag.f = f.ToString();
            return View();
        }

        [HttpGet]
        [Route("/Download/")]
        public async Task<ActionResult> GetFile(Guid f)
        {
            try
            {
                FileDto file = await _Api.GetFile(f);
                if (!string.IsNullOrEmpty(file.ErrorMessage))
                {
                    ViewBag.Error = file.ErrorMessage;
                    return View("Error");
                }
                return File(file.Payload, file.MimeType, file.FileName);
            }
            catch
            {
                return BadRequest();
            }            
        }
    }
}

