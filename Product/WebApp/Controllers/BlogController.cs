using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using UpDiddy.Api;
using UpDiddyLib.Dto;
using UpDiddy.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using UpDiddy.Services.ButterCMS;
using UpDiddy.ViewModels.ButterCMS;
using ButterCMS.Models;
using Microsoft.AspNetCore.Http;
using ButterCMS;


namespace UpDiddy.Controllers
{

    [Route("[controller]")]
    public class BlogController : BaseController
    {
        private readonly IConfiguration _configuration;
        private readonly IButterCMSService _butterService;
        private ButterCMSClient _butterCMSClient;

        public BlogController(IApi api, IConfiguration configuration, IButterCMSService butterService)
             : base(api)
        {
            _configuration = configuration;
            _butterService = butterService;
            _butterCMSClient = new ButterCMSClient(_configuration["ButterCMS:ReadApiToken"]);
        }

        [HttpGet]
        [Route("")]
        [Route("blog")]
        [Route("blog/p/{page}")]
        public async Task<IActionResult> IndexAsync(int page = 1)
        {
            var response = await _butterCMSClient.ListPostsAsync(page, 10);

            ViewBag.NextPage = response.Meta.NextPage;
            ViewBag.PreviousPage = response.Meta.PreviousPage;
            return View("Posts", response);
        }

        [HttpGet]
        [Route("/blog/{slug}")]
        public async Task<IActionResult> ShowPost(string slug)
        {
            var response = await _butterCMSClient.RetrievePostAsync(slug);
            return View("Post", response);
        }

        //[HttpGet]
        //[Route("/blog/tags/{slug}")]
        //public async Task<IActionResult> SarchbyTag(string tag)
        //{
        //    var response = await _butterCMSClient.RetrievePageAsync();
        //    return View("Post", response);
        //}
    }
}