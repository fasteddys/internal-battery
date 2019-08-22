using ButterCMS;
using ButterCMS.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using UpDiddy.ViewModels.ButterCMS;
using UpDiddyLib.Helpers;

namespace UpDiddy.Services.ButterCMS
{
    public class ButterCMSService : IButterCMSService
    {
        private ICacheService _cacheService = null;
        private IConfiguration _configuration = null;
        private ISysEmail _sysEmail = null;
        private ButterCMSClient _butterClient;
        private string CmsCacheKeyPrefix;

        public ButterCMSService(ICacheService cacheService, IConfiguration configuration, ISysEmail sysEmail)
        {
            _cacheService = cacheService;
            _configuration = configuration;
            _sysEmail = sysEmail;
            _butterClient = new ButterCMSClient(_configuration["ButterCMS:ReadApiToken"]);
            CmsCacheKeyPrefix = Constants.CMS.CACHE_KEY_PREFIX;
        }

        /// <summary>
        /// 
        /// Generic method to leverage the RetrieveContentFields method from ButterCMS
        /// to get BCMS collection using keys and query parameters. The resulting GET
        /// request will look similar to this example:
        /// 
        /// https://api.buttercms.com/v2/content/?keys=careercircle_public_site_navigation&levels=3&auth_token=*AUTH_TOKEN*
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="CacheKey">Key for the Redis cached version of the BCMS response</param>
        /// <param name="Keys">Any keys required to fetch the content</param>
        /// <param name="QueryParameters">Any additional query parameters to modify the GET request</param>
        /// <returns>An object in the form of T representing the BCMS response</returns>
        public async Task<T> RetrieveContentFieldsAsync<T>(string CacheKey, string[] Keys, Dictionary<string, string> QueryParameters) where T : class
        {
            T CachedButterResponse = await _cacheService.GetCachedValueAsync<T>(CmsCacheKeyPrefix + CacheKey);
            try
            {
                if (CachedButterResponse == null)
                {
                    CachedButterResponse = await _butterClient.RetrieveContentFieldsAsync<T>(Keys, QueryParameters);
                    
                    if(CachedButterResponse == null)
                    {
                        await SendEmailNotificationAsync(CacheKey);
                        return null;
                    }
                    await _cacheService.SetCachedValueAsync(CmsCacheKeyPrefix + CacheKey, CachedButterResponse);
                }
            }
            catch(ContentFieldObjectMismatchException)
            {
                await SendEmailNotificationAsync(CacheKey);
                return null;
            }
            return CachedButterResponse;
        }

        public async Task<PageResponse<T>> RetrievePageAsync<T>(string Url, Dictionary<string, string> QueryParameters = null) where T : ButterCMSBaseViewModel
        {
            string CacheKey = AssemblePageCacheKey(Constants.CMS.PAGE_CACHE_KEY_PREFIX + Url, QueryParameters);
            CMSResponseHelper<PageResponse<T>> ResponseHelper = await _cacheService.GetCachedValueAsync<CMSResponseHelper<PageResponse<T>>>(CacheKey);

            try
            {
                if (ResponseHelper == null)
                {
                    ResponseHelper = new CMSResponseHelper<PageResponse<T>>();
                    PageResponse<T> CachedButterResponse = await _butterClient.RetrievePageAsync<T>("*", DecipherCmsPageFromUrl(Url), QueryParameters);
                    ResponseHelper.Data = CachedButterResponse;

                    if (CachedButterResponse == null)
                        ResponseHelper.ResponseCode = Constants.CMS.NULL_RESPONSE;
                    else
                        ResponseHelper.ResponseCode = Constants.CMS.RESPONSE_RECEIVED;

                    await _cacheService.SetCachedValueAsync(CacheKey, ResponseHelper);
                }

            }
            catch (ContentFieldObjectMismatchException)
            {
                return null;
            }

            return ResponseHelper.Data;
            
        }

        public async Task<bool> ClearCachedPageAsync(string Slug)
        {
            string CacheKey = AssemblePageCacheKey(Constants.CMS.PAGE_CACHE_KEY_PREFIX + Slug);
            return await _cacheService.RemoveCachedValueAsync(CacheKey);
        }

        public async Task<bool> ClearCachedKeyAsync(string Key)
        {
            return await _cacheService.RemoveCachedValueAsync(Key);
        }

        private string AssemblePageCacheKey(string PageSlug, Dictionary<string, string> QueryParameters = null){
            Dictionary<string, string> QueryParamsFromUrl = ExtractQueryParamsFromUrlString(PageSlug);

            //Urls may come in the query params attached
            if(QueryParamsFromUrl != null){
                if(QueryParameters == null){
                    QueryParameters = new Dictionary<string, string>();
                }
                foreach(string key in QueryParamsFromUrl.Keys){
                    if(!QueryParameters.Keys.Contains(key))
                        QueryParameters.Add(key, QueryParamsFromUrl[key]);
                }
            }
            

            PageSlug = DecipherKeyRouteFromUrl(PageSlug);
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(CmsCacheKeyPrefix)
                .Append(PageSlug);

            // Check to see if this is a preview request.
            if(QueryParameters != null){
                foreach(string Key in QueryParameters.Keys){
                    if(Key.Equals("preview") && QueryParameters[Key].Equals("1"))
                        stringBuilder.Append("_preview");
                }
            }

            return stringBuilder.ToString();
        }

        private Dictionary<string, string> ExtractQueryParamsFromUrlString(string Url){
            int index = Url.IndexOf("?");
            string QueryParamString = string.Empty;
            if (index <= 0)
                return null;

            if(index + 1 >= Url.Length)
                return null;
            
            QueryParamString = Url.Substring(index + 1, Url.Length - index - 1);
            Dictionary<string, string> QueryParams = new Dictionary<string, string>();
            if(QueryParamString.Equals(string.Empty))
                return QueryParams;

            string[] split = QueryParamString.Split("&");
            
            foreach(string s in split){
                string[] param = s.Split("=");
                QueryParams.Add(param[0], param[1]);
            }
            return QueryParams;
        }

        private string DecipherCmsPageFromUrl(string Url){
            Url = DecipherKeyRouteFromUrl(Url);
            Url = Url.Split("_").Last().ToLower();
            return Url;
        }

        private string DecipherKeyRouteFromUrl(string Url){
            //Take out protocol annd domain from url
            Url =  Url.Replace(_configuration["Environment:BaseUrl"], "/").ToLower();

            //Edge case if home page is supplied without closing slash
            Url =  Url.Replace(_configuration["Environment:BaseUrl"].Substring(0, _configuration["Environment:BaseUrl"].Length - 1), "/").ToLower();

            //Strip any query params
            int index = Url.IndexOf("?");
            if (index > 0)
                Url = Url.Substring(0, index);

            //Turn slashes to underscores for cache formatting
            Url = Url.Replace("/", "_");

            return Url;
        }

        private async Task SendEmailNotificationAsync(string CacheKey)
        {
            /**
             * We're caching that we've sent this email to ensure that as traffic increases,
             * we don't spam the CareerCircle errors inbox upon navigation fetch failure.
             */
            string CacheKeyForNavigationLoadFailure = "HasSentNavigationLoadFailureEmail";
            string HasSentNotificationEmail = await _cacheService.GetCachedValueAsync<string>(CmsCacheKeyPrefix + CacheKeyForNavigationLoadFailure);
            if (string.IsNullOrEmpty(HasSentNotificationEmail))
            {
                StringBuilder HtmlMessage = new StringBuilder();
                HtmlMessage.Append("Error retrieving " + CacheKey + " from ButterCMS, or Redis. Falling back to error navigation.");
                await _sysEmail.SendEmailAsync(_configuration["ButterCMS:CareerCirclePublicSiteNavigation:FailedFetchNotifyEmail"],
                    "ALERT! Navigation failed to load.",
                    HtmlMessage.ToString(),
                    Constants.SendGridAccount.Transactional);
                await _cacheService.SetCachedValueAsync<string>(CmsCacheKeyPrefix + CacheKeyForNavigationLoadFailure, "true");
            }            
        }

        public async Task<XmlDocument> GetButterSitemapAsync(){
            XmlDocument xmlDocument = await _butterClient.GetSitemapAsync();
            return xmlDocument;
        }

        public async Task<IList<string>> GetBlogAuthorSlugsAsync(){
            IEnumerable<Author> Authors =  await _butterClient.ListAuthorsAsync(includeRecentPosts: true);
            IList<string> AuthorSlugs = new List<string>();
            foreach(Author author in Authors){
                if(author.RecentPosts.Count() > 0)
                    AuthorSlugs.Add(author.Slug);
            }
            return AuthorSlugs;
        }

        public async Task<IList<string>> GetBlogCategorySlugsAsync(){
            IEnumerable<Category> Categories =  await _butterClient.ListCategoriesAsync(includeRecentPosts: true);
            IList<string> CategoriesList = new List<string>();
            foreach(Category category in Categories){
                if(category.RecentPosts.Count() > 0)
                    CategoriesList.Add(category.Slug);
            }
            return CategoriesList;
        }

        public async Task<IList<string>> GetBlogTagSlugsAsync(){
            IEnumerable<Tag> Tags =  await _butterClient.ListTagsAsync(includeRecentPosts: true);
            IList<string> TagsList = new List<string>();
            foreach(Tag tag in Tags){
                if(tag.RecentPosts.Count() > 0)
                    TagsList.Add(tag.Slug);
            }
            return TagsList;
        }

        /*
            This method implementation is not ideal, as we are calling Butter to iterate through
            all blog post pages to get the count number. I don't see any way to get the total 
            number of Blog posts in Butter, so this was the only approach. We are leveraging the
            'excludeBody' parameter of the call to reduce payload size.
         */
        public async Task<int> GetNumberOfBlogPostPagesAsync(){
            PostsResponse posts = await _butterClient.ListPostsAsync(pageSize: Constants.CMS.BLOG_PAGINATION_PAGE_COUNT,
                excludeBody: true);  
            int NumberOfPages = 0;
            if(posts == null)
                return NumberOfPages;
            
            NumberOfPages++;
            int? NextPageExists = posts.Meta.NextPage;

            if(NextPageExists == null)
                return NumberOfPages;
            
            while(NextPageExists != null){
                NumberOfPages++;
                posts = await _butterClient.ListPostsAsync(page: NumberOfPages, pageSize: Constants.CMS.BLOG_PAGINATION_PAGE_COUNT,
                    excludeBody: true); 
                NextPageExists = posts.Meta.NextPage;
            }

            return NumberOfPages;
        }

        private class CMSResponseHelper<T>
        {
            public string ResponseCode { get; set; }
            public T Data { get; set; }
        }
    }
}
