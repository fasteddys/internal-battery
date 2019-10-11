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
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyLib.Dto.User;

namespace UpDiddyApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IdentityController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly ISubscriberService _subscriberService;
        private readonly IJobService _jobService;

        public IdentityController(IServiceProvider services)
        {
            _mapper = services.GetService<IMapper>();
            _userService = services.GetService<IUserService>();
            _subscriberService = services.GetService<ISubscriberService>();
            _jobService = services.GetService<IJobService>();
        }

        [HttpPost]
        public async Task<IActionResult> IsUserInAuth0([FromBody] UserDto userDto)
        {
            bool? isUserInAuth0 = null;

            // todo: secure this request using a secret stored in the key vault 
            if (false)
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var user = await _userService.GetUserByEmailAsync(userDto.Email);

            if(user != null)
            {
                return Ok();
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateUserAsync([FromBody] CreateUserDto createUserDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = _mapper.Map<CreateUserDto, User>(createUserDto);

            // the publicly available endpoint should not allow user creation without email verification nor does it allow special roles to be assigned
            var createLoginResponse = await _userService.CreateUserAsync(user, true, null);

            if (!createLoginResponse.Success)
            {
                return BadRequest(createLoginResponse.Message);
            }
            else
            {
                var createSubscriberResult = await _subscriberService.CreateSubscriberAsync(createUserDto);
                // if the subscriber is not created successfully, remove the associated login that was created and return a failure message
                if (!createSubscriberResult)
                {
                    _userService.DeleteUserAsync(user.UserId);
                    return BadRequest("An error occurred creating the user.");
                }
                else
                {
                    // Store the job referral code if one was supplied
                    if (createUserDto.JobReferralCode != null)
                    {
                        await _jobService.UpdateJobReferral(createUserDto.JobReferralCode, createUserDto.SubscriberGuid.ToString());
                    }
                    return Ok(createLoginResponse.Message);
                }
            }
        }
    }
}