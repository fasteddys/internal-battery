using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddy.ViewModels.Components.Layout;

namespace UpDiddy.Services.ButterCMS
{
    public interface IButterCMSService
    {
        T RetrieveContentFields<T>(string CacheKey, string[] Keys, Dictionary<string, string> QueryParameters) where T : class;
    }
}
