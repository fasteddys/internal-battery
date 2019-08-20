﻿using ButterCMS.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using UpDiddy.ViewModels.ButterCMS;
using System.Xml;
using Microsoft.AspNetCore.Http;

namespace UpDiddy.Services.ButterCMS
{
    public interface IButterCMSService
    {
        Task<T> RetrieveContentFieldsAsync<T>(string CacheKey, string[] Keys, Dictionary<string, string> QueryParameters) where T : class;
        Task<PageResponse<T>> RetrievePageAsync<T>(string CacheKey, string Slug, Dictionary<string, string> QueryParameters = null) where T : ButterCMSBaseViewModel;
        Task<bool> ClearCachedValueAsync(string CacheKey);
        Task<XmlDocument> GetButterSitemapAsync();
        Task<IList<string>> GetBlogAuthorSlugsAsync();
        Task<IList<string>> GetBlogCategorySlugsAsync();
        Task<IList<string>> GetBlogTagSlugsAsync();
        Task<int> GetNumberOfBlogPostPagesAsync();
        string AssembleCacheKey(string KeyPrefix, string PageSlug, IQueryCollection Query = null);
    }
}
