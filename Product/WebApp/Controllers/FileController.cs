using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using UpDiddy.ViewModels;
using UpDiddyLib.Dto;
using UpDiddyLib.Helpers.Braintree;
using UpDiddyLib.Helpers;
using Braintree;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using UpDiddy.Api;
using UpDiddy.Authentication;
using System.Threading.Tasks;
using UpDiddy.Services.ButterCMS;
using UpDiddy.ViewModels.ButterCMS;
using ButterCMS.Models;
using Microsoft.AspNetCore.Http;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UpDiddy.Controllers
{
    public class FileController : BaseController
    {
        private readonly IConfiguration _configuration;

        public FileController(IApi api, IConfiguration configuration) : base(api)
        {
            _configuration = configuration;

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

