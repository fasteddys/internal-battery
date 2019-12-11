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
    [Route("/V2/blog/")]
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
        [Route("page/{page?}")]
        public async Task<IActionResult> GetBlogs(int page = 1, int pageSize= 10)
        {
            var response = await _butterCMSService.GetBlogsAsync(page, pageSize); 
            return Ok(response);
        }
     

        [HttpGet]
        [Route("slug/{slug}")]
        public async Task<IActionResult> GetBlogBySlug(string slug)
        {
            var response = await _butterCMSService.GetBlogBySlugAsync(slug);
            return Ok(response);
        }

    
        [HttpGet]
        [Route("search")]
        public async Task<IActionResult> SearchBlogs(string query)
        {
               var response = await _butterCMSService.SearchBlogsAsync(query);
               return Ok(response);
        }

        [HttpGet]
        [Route("tag/{tag}")]
        public async Task<IActionResult> GetPostsByTag(string tag)
        {
            var response = await _butterCMSService.GetBlogsByTagAsync(tag);
            return Ok(response);
        }

        [HttpGet]
        [Route("category/{category}")]
        public async Task<IActionResult> GetPostsByCategory(string category)
        {
            var response = await _butterCMSService.GetBlogsByCategoryAsync(category);
            return Ok(response);
        }

        [HttpGet]
        [Route("author/{author}")]
        public async Task<IActionResult> GetPostsByAuthorAsync(string author)
        {
            var response = await _butterCMSService.GetBlogsByCategoryAsync(author);
            return Ok(response);
        }


        [HttpGet]
        [Route("site-map")]
        public async Task<IActionResult> GetButterSiteMap()
        {
            var rVal = await _butterCMSService.GetButterSitemapAsync();
            return Ok(rVal);
        }

        [HttpGet]
        [Route("author-slugs")]
        public async Task<IActionResult> GetBlogAuthorSlugs()
        {
            var rVal = await _butterCMSService.GetBlogAuthorSlugsAsync();
            return Ok(rVal);
        }

        [HttpGet]
        [Route("category-slugs")]
        public async Task<IActionResult> GetBlogCategorySlugs()
        {
            var rVal = await _butterCMSService.GetBlogCategorySlugsAsync();
            return Ok(rVal);
        }

        [HttpGet]
        [Route("tag-slugs")]
        public async Task<IActionResult> GetBlogTagSlugs()
        {
            var rVal = await _butterCMSService.GetBlogTagSlugsAsync();
            return Ok(rVal);
        }

        [HttpGet]
        [Route("page-count")]
        public async Task<IActionResult> GetBlogPageCount()
        {
            int rVal = await _butterCMSService.GetNumberOfBlogPostPagesAsync();
            return Ok(rVal);
        }




    }
}