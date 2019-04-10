using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddy.ViewModels.Components.Layout;

namespace UpDiddy.Services.ButterCMS
{
    public interface IButterCMSService
    {
        T GetResponse<T>(string ApiSlug, string CacheKey) where T : class;
    }
}
