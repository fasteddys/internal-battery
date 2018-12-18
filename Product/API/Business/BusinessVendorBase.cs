using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Net.Http;
using UpDiddyApi.Models;
using UpDiddyLib.Helpers;
using UpDiddyLib.MessageQueue;
using Microsoft.Extensions.Logging;
using UpDiddyApi.Business.Resume;

namespace UpDiddyApi.Business
{
    public class BusinessVendorBase
    {
        #region Class Members
        protected internal UpDiddyDbContext _db = null;
        protected internal IMapper _mapper;
        protected internal Microsoft.Extensions.Configuration.IConfiguration _configuration;
        protected internal string _queueConnection = string.Empty;
        //protected internal CCQueue _queue = null;
        protected internal string _apiBaseUri = String.Empty;
        protected internal string _accessToken = String.Empty;
        protected internal WozTransactionLog _translog = null;
        protected internal ILogger _syslog = null;
        protected IHttpClientFactory _httpClientFactory = null;
        protected internal ISovrenAPI _sovrenApi;
        #endregion
    }
}