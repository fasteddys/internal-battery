using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using UpDiddyApi.ApplicationCore.Services.Identity;
using UpDiddyApi.ApplicationCore.Services.Identity.Interfaces;
using UpDiddyApi.Controllers.Resources;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
namespace UpDiddyApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IdentityController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly ISubscriberService _subscriberService;

        public IdentityController(IServiceProvider services)
        {
            _mapper = services.GetService<IMapper>();
            _userService = services.GetService<IUserService>();
            _subscriberService = services.GetService<ISubscriberService>();
        }

        [HttpPost]
        public async Task<IActionResult> CreateUserAsync([FromBody] UserCredentialsResource userCredentials)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = _mapper.Map<UserCredentialsResource, User>(userCredentials);

            // the publicly available endpoint should not allow user creation without email verification nor does it allow special roles to be assigned
            var createLoginResponse = await _userService.CreateUserAsync(user, true, null);

            if (!createLoginResponse.Success)
            {
                return BadRequest(createLoginResponse.Message);
            }
            else
            {
                var createSubscriberResult = await _subscriberService.CreateSubscriberAsync(user, userCredentials.Group);
                // if the subscriber is not created successfully, remove the associated login that was created and return a failure message
                if (!createSubscriberResult)
                {
                    _userService.DeleteUserAsync(user.UserId);
                    return BadRequest("An error occurred creating the user.");
                }
                else
                {
                    return Ok(createLoginResponse.Message);
                }
            }
        }
    }
}