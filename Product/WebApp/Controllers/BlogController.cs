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
using UpDiddyLib.Helpers;


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
        [Route("/blog")]
        [Route("/blog/{page?}")]
        public async Task<IActionResult> IndexAsync(int page = 1)
        {
            var response = await _butterCMSClient.ListPostsAsync(page, 10);
            ViewBag.NextPage = response.Meta.NextPage;
            ViewBag.PreviousPage = response.Meta.PreviousPage;
            return View("Posts", response);
        }

        [HttpGet]
        [Route("/blog/post/{slug}")]
        public async Task<IActionResult> ShowPostAsync(string slug)
        {
            var response = await _butterCMSClient.RetrievePostAsync(slug);
            ViewData[Constants.Seo.META_TITLE] = response.Data.SeoTitle;
            ViewData[Constants.Seo.META_DESCRIPTION] = response.Data.MetaDescription;
            ViewData[Constants.Seo.OG_TITLE] = response.Data.SeoTitle + " - CareerCircle Blog";
            ViewData[Constants.Seo.OG_DESCRIPTION] = response.Data.MetaDescription;
            ViewData[Constants.Seo.OG_IMAGE] = response.Data.FeaturedImage;
            return View("Post", response);
        }

        [HttpGet]
        [Route("/blog/search")]
        public async Task<IActionResult> SearchPostsAsync(string query)
        {
            PostsResponse response;
            if (!string.IsNullOrEmpty(query))
            {
                response = await _butterCMSClient.SearchPostsAsync(query: query);
            }
            else
            {
                response = await _butterCMSClient.ListPostsAsync(1, 10);
            }
            return View("Posts", response);
        }

        [HttpGet]
        [Route("/blog/tag/{tag}")]
        public async Task<IActionResult> GetPostsByTagAsync(string tag)
        {
            PostsResponse response = await _butterCMSClient.ListPostsAsync(tagSlug: tag);
            return View("Posts", response);
        }

        [HttpGet]
        [Route("/blog/category/{category}")]
        public async Task<IActionResult> GetPostsByCategoryAsync(string category)
        {
            PostsResponse response = await _butterCMSClient.ListPostsAsync(categorySlug: category);
            return View("Posts", response);
        }

        [HttpGet]
        [Route("/blog/author/{author}")]
        public async Task<IActionResult> GetPostsByAuthorAsync(string author)
        {
            ViewData["ShowAuthorInfo"]=true;
            PostsResponse response = await _butterCMSClient.ListPostsAsync(authorSlug: author);
            return View("Posts", response);
        }
    }
}