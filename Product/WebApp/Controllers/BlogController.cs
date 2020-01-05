using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using UpDiddy.Api;
using System.Threading.Tasks;
using UpDiddy.Services.ButterCMS;
using ButterCMS.Models;
using ButterCMS;
using UpDiddyLib.Helpers;


namespace UpDiddy.Controllers
{
    [Route("[controller]")]
    public class BlogController : BaseController
    {
     
        private readonly IButterCMSService _butterService;
        private ButterCMSClient _butterCMSClient;

        public BlogController(IApi api, IConfiguration configuration, IButterCMSService butterService)
             : base(api,configuration)
        {
            _butterService = butterService;
            _butterCMSClient = new ButterCMSClient(_configuration["ButterCMS:ReadApiToken"]);
        }

        /*
     [HttpGet]
     [Route("/blog")]
     [Route("/blog/{page?}")]
     public async Task<IActionResult> IndexAsync(int page = 1)
     {
         var response = await _butterCMSClient.ListPostsAsync(page, Constants.CMS.BLOG_PAGINATION_PAGE_COUNT);
         ViewBag.NextPage = response.Meta.NextPage;
         ViewBag.PreviousPage = response.Meta.PreviousPage;
         return View("Posts", response);
     }

     [HttpGet]
     [Route("/blog/post/{slug}")]
     public async Task<IActionResult> ShowPostAsync(string slug)
     {
         var response = await _butterCMSClient.RetrievePostAsync(slug);

         string Keywords = string.Empty;
         bool isFirst = true;
         foreach(Category c in response.Data.Categories)
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
         ViewData["ShowAuthorInfo"]=true;
         PostsResponse response = await _butterCMSClient.ListPostsAsync(authorSlug: author);
         if(response?.Data?.Count() == 0)
             return NotFound();
         return View("Posts", response);
     }
     */
    }
}