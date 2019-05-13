using ButterCMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddy.ViewModels.ButterCMS;
using UpDiddy.ViewModels.Components.Layout;

namespace UpDiddy.Services.ButterCMS
{
    public interface IButterCMSService
    {
        T RetrieveContentFields<T>(string CacheKey, string[] Keys, Dictionary<string, string> QueryParameters) where T : class;
        PageResponse<T> RetrievePage<T>(string CacheKey, string Slug, Dictionary<string, string> QueryParameters = null) where T : ButterCMSBaseViewModel;
        bool ClearCachedValue<T>(string CacheKey);
    }
}
