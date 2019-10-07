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
        public async Task<ActionResult> GetFile(Guid f)
        {
            FileDto file = await _Api.GetFile(f);
            return File( file.Payload, file.MimeType, file.FileName);
        }

    }
}

