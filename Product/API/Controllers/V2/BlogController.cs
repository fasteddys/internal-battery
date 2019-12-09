using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UpDiddyApi.ApplicationCore.Services;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using AutoMapper;
using System.Security.Claims;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using Microsoft.Extensions.DependencyInjection;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using Microsoft.AspNetCore.Http;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyLib.Domain.Models;
using UpDiddyApi.ApplicationCore.Exceptions;
using UpDiddyLib.Shared.GoogleJobs;
using System.Collections.Generic;
using UpDiddyLib.Helpers;

namespace UpDiddyApi.Controllers.V2
{
    [Route("/V2/butter/")]
    [ApiController]
    public class BlogController : ControllerBase
    {

        private readonly IMapper _mapper;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private readonly ILogger _syslog;
        private readonly IServiceProvider _services;
        private readonly IButterCMSService _butterCMSService;


        #region constructor 
        public BlogController(IServiceProvider services
        , IJobAlertService jobAlertService
        , IJobFavoriteService jobFavoriteService
        , IJobSearchService jobSearchService
        , ICloudTalentService cloudTalentService
        , ITrackingService trackingService
        , IKeywordService keywordService
        , IButterCMSService butterCMSService)

        {
            _services = services;
            _mapper = _services.GetService<IMapper>();
            _configuration = _services.GetService<Microsoft.Extensions.Configuration.IConfiguration>();
            _syslog = _services.GetService<ILogger<JobController>>();
            _butterCMSService = butterCMSService;
        }
        #endregion

        [HttpGet]
        [Route("/blog")]
        [Route("/blog/{page?}")]
        public async Task<IActionResult> IndexAsync(int page = 1)
        {
            var response = await _butterCMSService.GetBlogs(page, Constants.CMS.BLOG_PAGINATION_PAGE_COUNT); 
            return Ok(response);
        }


        /*

        [HttpGet]
        [Route("/blog/post/{slug}")]
        public async Task<IActionResult> ShowPostAsync(string slug)
        {
            var response = await _butterCMSClient.RetrievePostAsync(slug);

            string Keywords = string.Empty;
            bool isFirst = true;
            foreach (Category c in response.Data.Categories)
            {
                if (isFirst)
                {
                    Keywords += c.Name;
                    isFirst = false;
                }
                else
                    Keywords += ", " + c.Name;
            }

            ViewData[Constants.Seo.TITLE] = response.Data.SeoTitle + Constants.CMS.BLOG_TITLE_TAG_SUFFIX;
            ViewData[Constants.Seo.META_DESCRIPTION] = response.Data.MetaDescription;
            ViewData[Constants.Seo.META_KEYWORDS] = Keywords;
            ViewData[Constants.Seo.OG_TITLE] = response.Data.SeoTitle + Constants.CMS.BLOG_TITLE_TAG_SUFFIX;
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
            ViewData["ShowAuthorInfo"] = true;
            PostsResponse response = await _butterCMSClient.ListPostsAsync(authorSlug: author);
            if (response?.Data?.Count() == 0)
                return NotFound();
            return View("Posts", response);
        }

    */


    }
}