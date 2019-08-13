using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using UpDiddyApi.Authorization;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyLib.Helpers;
using System.IO;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Factory;
using System.Data.SqlClient;
using AutoMapper.QueryableExtensions;
using System.Data;
using System.Web;
using UpDiddyLib.Dto.Marketing;
using UpDiddyLib.Shared;
using Hangfire;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;

namespace UpDiddyApi.Controllers
{
    [Route("api/[controller]")]
    public class GroupsController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly ILogger _syslog;
        private readonly IRepositoryWrapper _repositoryWrapper;


        public GroupsController(IMapper mapper,
            IConfiguration configuration,
            ILogger<SubscriberController> sysLog,
            IRepositoryWrapper repositoryWrapper)
        {
            _mapper = mapper;
            _configuration = configuration;
            _syslog = sysLog;
            _repositoryWrapper = repositoryWrapper;
        }

        [Authorize(Policy = "IsCareerCircleAdmin")]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            IEnumerable<Group> ieGroups = await _repositoryWrapper.GroupRepository.GetAllAsync();
            IList<Group> Groups = ieGroups.ToList();
            return Ok(Groups);
        }
    }
}
