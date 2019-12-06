using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using AutoMapper;
using Microsoft.Extensions.Logging;
using UpDiddyApi.Models;
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
        public IActionResult Get()
        {
            IEnumerable<Group> ieGroups = _repositoryWrapper.GroupRepository.GetAll();
            IList<Group> Groups = ieGroups.ToList();
            return Ok(Groups);
        }
    }
}
