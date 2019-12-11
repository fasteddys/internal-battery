using ButterCMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace UpDiddyApi.ApplicationCore.Interfaces
{
    public interface IButterCMSService
    {

        Task<T> RetrieveContentFieldsAsync<T>(string[] Keys, Dictionary<string, string> QueryParameters) where T : class; 
        Task<PageResponse<T>> RetrievePageAsync<T>(string Slug, Dictionary<string, string> QueryParameters = null) where T : class;
        Task<bool> ClearCachedPageAsync(string Slug);
        Task<bool> ClearCachedKeyAsync(string Key);
        Task<XmlDocument> GetButterSitemapAsync();
        Task<IList<string>> GetBlogAuthorSlugsAsync();
        Task<IList<string>> GetBlogCategorySlugsAsync();
        Task<IList<string>> GetBlogTagSlugsAsync();
        Task<int> GetNumberOfBlogPostPagesAsync();

        Task<PostsResponse> GetBlogsAsync(int pageNum, int pageSize);
        Task<PostResponse> GetBlogBySlugAsync(string slug);
        Task<PostsResponse> SearchBlogsAsync(string slug);
        Task<PostsResponse> GetBlogsByTagAsync(string tag);
        Task<PostsResponse> GetBlogsByCategoryAsync(string tag);
        Task<PostsResponse> GetBlogsByAuthorAsync(string tag);
    }

}
