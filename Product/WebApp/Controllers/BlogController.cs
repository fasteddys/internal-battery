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
        public async Task<IActionResult> IndexAsync()
        {
            var response = await _butterCMSClient.ListPostsAsync(1, 10);
            ViewBag.Posts = response.Data;
            ViewBag.NextPage = response.Meta.NextPage;
            ViewBag.PreviousPage = response.Meta.PreviousPage;
            return View("Posts");
        }

        [HttpGet]
        [Route("/blog/{slug}")]
        public async Task<IActionResult> ShowPost(string slug)
        {
            var response = await _butterCMSClient.RetrievePostAsync(slug);
            ViewBag.Post = response.Data;
            return View("Post");
        }

        [Route("")]
        [Route("blog")]
        [Route("blog/p/{page}")]
        public async Task<IActionResult> ListAllPosts(int page = 1)
        {
            var response = await _butterCMSClient.ListPostsAsync(page, 10);
            ViewBag.Posts = response.Data;
            ViewBag.NextPage = response.Meta.NextPage;
            ViewBag.PreviousPage = response.Meta.PreviousPage;
            return View("Posts");
        }

       
    }
}
