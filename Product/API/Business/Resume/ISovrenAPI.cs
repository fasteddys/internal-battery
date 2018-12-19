using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace UpDiddyApi.Business.Resume
{
    public interface ISovrenAPI
    {
        Task<String> SubmitResumeAsync(string base64Resume);
    }
}