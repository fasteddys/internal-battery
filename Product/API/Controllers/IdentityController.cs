using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UpDiddyApi.ApplicationCore.Services.Auth0;
using UpDiddyApi.ApplicationCore.Services.Auth0.Interfaces;

namespace UpDiddyApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IdentityController : ControllerBase
    {
        private readonly ILogger _syslog;
        private readonly IUserService _userService;

        public IdentityController(ILogger<IdentityController> sysLog, IUserService userService)
        {
            _syslog = sysLog;
            _userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateUserAsync([FromBody] User user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var user = 
            throw new NotImplementedException();
        }
    }
}