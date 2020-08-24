using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Services.JobDataMining
{
    public abstract class BaseProcess
    {
        protected JobSite _jobSite;
        protected internal ILogger _syslog = null;
        protected Guid _companyGuid = Guid.Empty;
        protected IConfiguration _configuration;
        protected IEmploymentTypeService _employmentTypeService;
        protected long? _totalBytesReceived;
        protected int? _successfulWebRequests;
        protected int? _unsuccessfulWebRequests;

        public BaseProcess(JobSite jobSite, ILogger logger, Guid companyGuid, IConfiguration configuration, IEmploymentTypeService employmentTypeService)
        {
            _syslog = logger;
            _jobSite = jobSite;
            _companyGuid = companyGuid;
            _configuration = configuration;
            _employmentTypeService = employmentTypeService;
        }

        /// <summary>
        /// This method sends a web request and retries if the response looks to be a transient failure (502 or 403).
        /// </summary>
        /// <param name="client"></param>
        /// <param name="uri"></param>
        /// <param name="retryCount"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> ProxyWebRequestWithRetry(HttpClient client, Uri uri, int retryCount = 0)
        {
            // exit early if we have exceeded max retries
            if(retryCount >= this._jobSite.MaxRetries)
            {
                _syslog.LogError($"Exceeded max retries for {_jobSite.Name} process calling {uri.ToString()}");
                return null;
            }
            
            var response = await client.GetAsync(uri);
            if (!response.IsSuccessStatusCode && (response.StatusCode == HttpStatusCode.BadGateway || response.StatusCode == HttpStatusCode.Forbidden))
            {
                // only retry if the response looks to be an issue with the proxy service or an IP block from the job site
                _syslog.LogError($"Unsuccessful response for {_jobSite.Name} process calling {uri.ToString()}; Status code: {((int)response.StatusCode).ToString()}, ReasonPhrase: {response.ReasonPhrase}");
                return await this.ProxyWebRequestWithRetry(client, uri, ++retryCount);
            }
            else
            {
                return response;
            }
        }
    }
}
