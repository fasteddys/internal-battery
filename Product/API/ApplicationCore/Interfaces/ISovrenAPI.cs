using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace UpDiddyApi.ApplicationCore.Interfaces
{
    public interface ISovrenAPI
    {
        Task<String> SubmitResumeAsync(int SubscriberId, string base64Resume);
    }
}